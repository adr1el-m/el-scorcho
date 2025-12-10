using Microsoft.EntityFrameworkCore;
using WHRID.Domain.Entities;

namespace WHRID.Infrastructure.DbContexts
{
    public class WHRIDDbContext : DbContext
    {
        public WHRIDDbContext(DbContextOptions<WHRIDDbContext> options) : base(options)
        {
        }

        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<RiskProfile> RiskProfiles { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<UTxO> UTxOs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Wallet>()
                .HasIndex(w => w.Address)
                .IsUnique();

            modelBuilder.Entity<Transaction>()
                .HasKey(t => t.Hash); // Assuming Hash is unique enough for this demo or use composite

            modelBuilder.Entity<UTxO>()
                .HasKey(u => new { u.TxHash, u.Index });
        }
    }
}
