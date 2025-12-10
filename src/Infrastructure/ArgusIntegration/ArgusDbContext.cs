using Microsoft.EntityFrameworkCore;
using WHRID.Infrastructure.ArgusIntegration.Entities;

namespace WHRID.Infrastructure.ArgusIntegration
{
    public class ArgusDbContext : DbContext
    {
        public ArgusDbContext(DbContextOptions<ArgusDbContext> options) : base(options)
        {
        }

        public DbSet<ArgusBlock> Blocks { get; set; }
        public DbSet<ArgusTransaction> Transactions { get; set; }
        public DbSet<ArgusTxOut> TxOuts { get; set; }
        public DbSet<ArgusTxIn> TxIns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Map to existing tables - standard Cardano DB Sync schema style
            // Note: Argus schema might differ slightly, but this is a standard starting point
            modelBuilder.Entity<ArgusBlock>().ToTable("block");
            modelBuilder.Entity<ArgusTransaction>().ToTable("tx");
            modelBuilder.Entity<ArgusTxOut>().ToTable("tx_out");
            modelBuilder.Entity<ArgusTxIn>().ToTable("tx_in");

            // Relationships
            modelBuilder.Entity<ArgusTransaction>()
                .HasOne(t => t.Block)
                .WithMany()
                .HasForeignKey(t => t.BlockId);

            modelBuilder.Entity<ArgusTxOut>()
                .HasOne(o => o.Transaction)
                .WithMany(t => t.Outputs)
                .HasForeignKey(o => o.TxId);

            modelBuilder.Entity<ArgusTxIn>()
                .HasOne(i => i.ConsumingTransaction)
                .WithMany(t => t.Inputs)
                .HasForeignKey(i => i.TxInId);
        }
    }
}
