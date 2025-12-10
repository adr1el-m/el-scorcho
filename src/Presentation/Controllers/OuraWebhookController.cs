using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WHRID.Infrastructure.ArgusIntegration;
using WHRID.Infrastructure.ArgusIntegration.Entities;
using Microsoft.EntityFrameworkCore;

namespace WHRID.Presentation.Controllers
{
    [ApiController]
    [Route("api/webhook/oura")]
    public class OuraWebhookController : ControllerBase
    {
        private readonly IDbContextFactory<ArgusDbContext> _contextFactory;
        private readonly ILogger<OuraWebhookController> _logger;

        public OuraWebhookController(IDbContextFactory<ArgusDbContext> contextFactory, ILogger<OuraWebhookController> logger)
        {
            _contextFactory = contextFactory;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveEvent([FromBody] JsonElement payload)
        {
            try 
            {
                using var context = await _contextFactory.CreateDbContextAsync();

                var handled = false;

                if (payload.TryGetProperty("block", out _))
                {
                    await ProcessBlockAsync(context, payload);
                    handled = true;
                }
                else if (payload.TryGetProperty("transaction", out _))
                {
                    await ProcessTransactionAsync(context, payload);
                    handled = true;
                }
                else if (payload.TryGetProperty("tx", out _))
                {
                    await ProcessTransactionAsync(context, payload);
                    handled = true;
                }

                if (handled)
                {
                    await context.SaveChangesAsync();
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Oura event");
                return StatusCode(500);
            }
        }

        private async Task ProcessBlockAsync(ArgusDbContext context, JsonElement payload)
        {
            if (!payload.TryGetProperty("block", out var blockData)) return;
            if (!payload.TryGetProperty("context", out var ctx)) return;

            string hashStr = blockData.GetProperty("hash").GetString();
            byte[] hash = Convert.FromHexString(hashStr.ToUpper());

            // Check if exists
            if (await context.Blocks.AnyAsync(b => b.Hash == hash)) return;

            var block = new ArgusBlock
            {
                Hash = hash,
                BlockNo = blockData.TryGetProperty("number", out var num) ? num.GetInt64() : 0,
                SlotNo = blockData.TryGetProperty("slot", out var slot) ? slot.GetInt64() : 0,
                EpochNo = ctx.TryGetProperty("epoch", out var epoch) ? epoch.GetInt32() : 0,
                Time = DateTime.UtcNow // Oura might not give time in this event, use UtcNow or parse if available
            };

            // Try to parse timestamp if available
            if (ctx.TryGetProperty("timestamp", out var ts))
            {
                // Oura timestamp is usually unix seconds
                block.Time = DateTimeOffset.FromUnixTimeSeconds(ts.GetInt64()).UtcDateTime;
            }

            context.Blocks.Add(block);
        }

        private async Task ProcessTransactionAsync(ArgusDbContext context, JsonElement payload)
        {
            if (!payload.TryGetProperty("transaction", out var txData)) return;
            if (!payload.TryGetProperty("context", out var ctx)) return;

            string txHashStr = txData.TryGetProperty("hash", out var hashEl) ? hashEl.GetString() : (ctx.TryGetProperty("tx_hash", out var ctxHash) ? ctxHash.GetString() : null);
            if (string.IsNullOrEmpty(txHashStr)) return;
            byte[] txHash = Convert.FromHexString(txHashStr.ToUpper());

            // Avoid duplicates
            if (await context.Transactions.AnyAsync(t => t.Hash == txHash)) return;

            // Ensure Block Exists
            string blockHashStr = ctx.TryGetProperty("block_hash", out var bhEl) ? bhEl.GetString() : null;
            if (string.IsNullOrEmpty(blockHashStr))
            {
                if (txData.TryGetProperty("block", out var blkStrEl))
                {
                    blockHashStr = blkStrEl.GetString();
                }
            }
            if (string.IsNullOrEmpty(blockHashStr)) return;
            byte[] blockHash = Convert.FromHexString(blockHashStr.ToUpper());
            
            var block = await context.Blocks.FirstOrDefaultAsync(b => b.Hash == blockHash);
            if (block == null)
            {
                // Create dummy block if missing (so FK doesn't fail)
                block = new ArgusBlock
                {
                    Hash = blockHash,
                    BlockNo = ctx.TryGetProperty("block_number", out var bn) ? bn.GetInt64() : 0,
                    SlotNo = ctx.TryGetProperty("slot", out var s) ? s.GetInt64() : 0,
                    Time = DateTime.UtcNow
                };
                if (ctx.TryGetProperty("timestamp", out var ts))
                {
                    block.Time = DateTimeOffset.FromUnixTimeSeconds(ts.GetInt64()).UtcDateTime;
                }
                context.Blocks.Add(block);
                // Save immediately to get Id for FK
                await context.SaveChangesAsync(); 
            }

            var tx = new ArgusTransaction
            {
                Hash = txHash,
                Block = block,
                BlockId = block.Id,
                BlockIndex = txData.TryGetProperty("index", out var idxEl) ? idxEl.GetInt32() : 0,
                Fee = txData.TryGetProperty("fee", out var feeEl) ? feeEl.GetDecimal() : (txData.TryGetProperty("fees", out var feesEl) ? feesEl.GetDecimal() : 0)
            };

            context.Transactions.Add(tx);
            await context.SaveChangesAsync(); // Save to get tx.Id

            // Process Outputs
            if (txData.TryGetProperty("outputs", out var outputs))
            {
                int index = 0;
                foreach (var output in outputs.EnumerateArray())
                {
                    var addr = output.TryGetProperty("address", out var addrEl) ? addrEl.GetString() : null;
                    if (string.IsNullOrEmpty(addr))
                    {
                        continue;
                    }

                    decimal value = 0;
                    if (output.TryGetProperty("amount", out var amtEl))
                    {
                        if (amtEl.ValueKind == JsonValueKind.Number)
                        {
                            value = amtEl.GetDecimal();
                        }
                        else if (amtEl.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var a in amtEl.EnumerateArray())
                            {
                                var unit = a.TryGetProperty("unit", out var uEl) ? uEl.GetString() : null;
                                var qtyStr = a.TryGetProperty("quantity", out var qEl) ? qEl.GetString() : null;
                                if (unit == "lovelace" && qtyStr != null && decimal.TryParse(qtyStr, out var qty))
                                {
                                    value = qty;
                                    break;
                                }
                            }
                        }
                    }
                    else if (output.TryGetProperty("value", out var valEl) && valEl.ValueKind == JsonValueKind.Number)
                    {
                        value = valEl.GetDecimal();
                    }

                    var txOut = new ArgusTxOut
                    {
                        Transaction = tx,
                        TxId = tx.Id,
                        Index = output.TryGetProperty("output_index", out var outIdxEl) ? outIdxEl.GetInt32() : index,
                        Address = addr,
                        Value = value,
                    };
                    context.TxOuts.Add(txOut);
                    index++;
                }
            }
        }
    }
}
