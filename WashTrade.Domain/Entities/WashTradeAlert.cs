using System;

namespace WashTrade.Domain.Entities
{
    public enum WashTradeType
    {
        CircularPattern,
        SelfFunding,
        Other
    }

    public class WashTradeAlert
    {
        public int Id { get; set; }
        
        public int WalletId { get; set; }
        public Wallet? Wallet { get; set; }

        public int? AssetId { get; set; }
        public Asset? Asset { get; set; }

        public WashTradeType Type { get; set; }
        public double ConfidenceScore { get; set; } // 0.0 to 1.0
        public DateTime DetectedAt { get; set; }
        public string Details { get; set; } = string.Empty; // JSON blob for specific details
    }
}
