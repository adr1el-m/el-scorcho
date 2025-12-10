using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WHRID.Application.Interfaces;
using WHRID.Domain.Entities;
using WHRID.Infrastructure.ArgusIntegration.Entities;

namespace WHRID.Infrastructure.ArgusIntegration
{
    public class ArgusRepository : IArgusRepository
    {
        private readonly IDbContextFactory<ArgusDbContext> _contextFactory;

        public ArgusRepository(IDbContextFactory<ArgusDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<List<Transaction>> GetTransactionsByAddressAsync(string address, int limit = 50)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // 1. Find TxIds where this address was an output (received funds)
            var receivedTxIds = await context.TxOuts
                .Where(o => o.Address == address)
                .OrderByDescending(o => o.Id)
                .Select(o => o.TxId)
                .Take(limit)
                .ToListAsync();

            // 2. Fetch full transactions
            var argusTxs = await context.Transactions
                .Include(t => t.Block)
                .Include(t => t.Outputs)
                .Include(t => t.Inputs)
                .Where(t => receivedTxIds.Contains(t.Id))
                .OrderByDescending(t => t.Id)
                .ToListAsync();

            // 3. Map to Domain Entities
            return argusTxs.Select(t => new Transaction
            {
                Hash = t.Hash != null ? Convert.ToHexString(t.Hash) : string.Empty,
                BlockHeight = t.Block?.BlockNo ?? 0,
                Timestamp = t.Block?.Time ?? DateTime.MinValue,
                Fee = t.Fee,
                IsOutgoing = false // For now, we only fetched received. To do outgoing, we need to check inputs.
            }).ToList();
        }

        public async Task<decimal> GetBalanceAsync(string address)
        {
            using var context = await _contextFactory.CreateDbContextAsync();
            
            // Simplified balance check (sum of all outputs for this address)
            // In reality, we need to subtract spent outputs.
            // For hackathon/demo with Argus, we might assume a view or indexer table for balances.
            // Here we just sum outputs for simplicity if no better option.
            try 
            {
                return await context.TxOuts
                    .Where(o => o.Address == address)
                    .SumAsync(o => o.Value);
            }
            catch
            {
                return 0;
            }
        }
        public async Task<List<Transaction>> GetRecentTransactionsAsync(int limit = 10)
        {
            using var context = await _contextFactory.CreateDbContextAsync();

            var argusTxs = await context.Transactions
                .Include(t => t.Block)
                .OrderByDescending(t => t.Id)
                .Take(limit)
                .ToListAsync();

            return argusTxs.Select(t => new Transaction
            {
                Hash = t.Hash != null ? Convert.ToHexString(t.Hash) : string.Empty,
                BlockHeight = t.Block?.BlockNo ?? 0,
                Timestamp = t.Block?.Time ?? DateTime.MinValue,
                Fee = t.Fee,
                IsOutgoing = false 
            }).ToList();
        }
    }
}
