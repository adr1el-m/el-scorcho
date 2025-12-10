using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WashTrade.Domain;
using WashTrade.Domain.Entities;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WashTrade.Analyzer
{
    public class Detective : IDetective
    {
        private readonly ILogger<Detective> _logger;

        public Detective(ILogger<Detective> logger)
        {
            _logger = logger;
        }

        public async Task DetectCircularTrading(WashTradeDbContext db, CancellationToken ct)
        {
            var cutoff = DateTime.UtcNow.AddHours(-24);
            
            var recentTxs = await db.Transactions
                .Include(t => t.SenderWallet)
                .Include(t => t.ReceiverWallet)
                .Include(t => t.Asset)
                .Where(t => t.Timestamp >= cutoff)
                .ToListAsync(ct);

            foreach (var tx1 in recentTxs)
            {
                if (tx1.SenderWallet == null || tx1.ReceiverWallet == null || tx1.SenderWalletId == null || tx1.AssetId == null) continue;

                var backTx = await db.Transactions
                    .Where(t => t.SenderWalletId == tx1.ReceiverWalletId && 
                                t.ReceiverWalletId == tx1.SenderWalletId &&
                                t.AssetId == tx1.AssetId && 
                                t.Timestamp > tx1.Timestamp &&
                                t.Timestamp <= tx1.Timestamp.AddHours(24))
                    .FirstOrDefaultAsync(ct);

                if (backTx != null)
                {
                    _logger.LogWarning($"Circular Trade Detected: {tx1.SenderWallet.Address} <-> {tx1.ReceiverWallet.Address} for Asset {tx1.AssetId}");

                    var exists = await db.WashTradeAlerts.AnyAsync(a => 
                        a.WalletId == tx1.SenderWalletId && 
                        a.AssetId == tx1.AssetId && 
                        a.Type == WashTradeType.CircularPattern &&
                        a.DetectedAt > DateTime.UtcNow.AddHours(-1), ct);

                    if (!exists)
                    {
                        var alert = new WashTradeAlert
                        {
                            WalletId = tx1.SenderWalletId.Value,
                            AssetId = tx1.AssetId,
                            Type = WashTradeType.CircularPattern,
                            ConfidenceScore = 0.9,
                            DetectedAt = DateTime.UtcNow,
                            Details = $"Circular trade detected between {tx1.SenderWallet.Address} and {tx1.ReceiverWallet.Address}. Tx1: {tx1.TxHash}, Tx2: {backTx.TxHash}"
                        };
                        db.WashTradeAlerts.Add(alert);
                        await db.SaveChangesAsync(ct);
                    }
                }
            }
        }

        public async Task DetectSelfFunding(WashTradeDbContext db, CancellationToken ct)
        {
            var cutoff = DateTime.UtcNow.AddHours(-24);
            var recentSales = await db.Transactions
                .Include(t => t.SenderWallet)
                .Include(t => t.ReceiverWallet)
                .Include(t => t.Asset)
                .Where(t => t.Timestamp >= cutoff && t.AssetId != null)
                .ToListAsync(ct);

            foreach (var saleTx in recentSales)
            {
                if (saleTx.SenderWallet == null || saleTx.ReceiverWallet == null || saleTx.SenderWalletId == null || saleTx.ReceiverWalletId == null) continue;

                var fundingTx = await db.Transactions
                    .Where(t => t.SenderWalletId == saleTx.ReceiverWalletId && 
                                t.ReceiverWalletId == saleTx.SenderWalletId && 
                                t.AssetId == null && 
                                t.Timestamp < saleTx.Timestamp &&
                                t.Timestamp >= saleTx.Timestamp.AddHours(-24))
                    .OrderByDescending(t => t.Timestamp)
                    .FirstOrDefaultAsync(ct);

                if (fundingTx != null)
                {
                     _logger.LogWarning($"Self-Funding Detected: {saleTx.ReceiverWallet.Address} funded {saleTx.SenderWallet.Address} before buying Asset {saleTx.AssetId}");

                    var exists = await db.WashTradeAlerts.AnyAsync(a => 
                        a.WalletId == saleTx.ReceiverWalletId && 
                        a.AssetId == saleTx.AssetId && 
                        a.Type == WashTradeType.SelfFunding &&
                        a.DetectedAt > DateTime.UtcNow.AddHours(-1), ct);

                    if (!exists)
                    {
                        var alert = new WashTradeAlert
                        {
                            WalletId = saleTx.ReceiverWalletId.Value,
                            AssetId = saleTx.AssetId,
                            Type = WashTradeType.SelfFunding,
                            ConfidenceScore = 0.85,
                            DetectedAt = DateTime.UtcNow,
                            Details = $"Self-Funding detected. Wallet {saleTx.ReceiverWallet.Address} funded {saleTx.SenderWallet.Address} (Tx: {fundingTx.TxHash}) before buying asset (Tx: {saleTx.TxHash})."
                        };
                        db.WashTradeAlerts.Add(alert);
                        await db.SaveChangesAsync(ct);
                    }
                }
            }
        }
    }
}
