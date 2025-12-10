using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WHRID.Application.Interfaces;
using WHRID.Domain.Entities;
using WHRID.Infrastructure.DbContexts;

namespace WHRID.Infrastructure.Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly WHRIDDbContext _context;

        public WalletRepository(WHRIDDbContext context)
        {
            _context = context;
        }

        public async Task<Wallet?> GetByAddressAsync(string address)
        {
            return await _context.Wallets
                .Include(w => w.RiskProfile)
                .Include(w => w.Transactions)
                .Include(w => w.UTxOs)
                .FirstOrDefaultAsync(w => w.Address == address);
        }

        public async Task SaveAsync(Wallet wallet)
        {
            if (_context.Entry(wallet).State == EntityState.Detached)
            {
                _context.Wallets.Add(wallet);
            }
            await _context.SaveChangesAsync();
        }
    }
}
