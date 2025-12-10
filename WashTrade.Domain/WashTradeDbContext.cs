using Microsoft.EntityFrameworkCore;
using WashTrade.Domain.Entities;

namespace WashTrade.Domain
{
    public class WashTradeDbContext : DbContext
    {
        public WashTradeDbContext(DbContextOptions<WashTradeDbContext> options) : base(options)
        {
        }

        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<WashTradeAlert> WashTradeAlerts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Wallet
            modelBuilder.Entity<Wallet>()
                .HasIndex(w => w.Address)
                .IsUnique();

            // Asset
            modelBuilder.Entity<Asset>()
                .HasIndex(a => new { a.PolicyId, a.AssetName })
                .IsUnique();

            // Transaction
            modelBuilder.Entity<Transaction>()
                .HasIndex(t => t.TxHash);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.SenderWallet)
                .WithMany(w => w.SentTransactions)
                .HasForeignKey(t => t.SenderWalletId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ReceiverWallet)
                .WithMany(w => w.ReceivedTransactions)
                .HasForeignKey(t => t.ReceiverWalletId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
