using System;

namespace WHRID.Domain.Entities
{
    public class Transaction
    {
        public string Hash { get; set; } = string.Empty;
        public long BlockHeight { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Fee { get; set; }
        
        public Guid WalletId { get; set; }
        public Wallet? Wallet { get; set; }
        
        public bool IsOutgoing { get; set; } // Relative to the wallet
    }
}
