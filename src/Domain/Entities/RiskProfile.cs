using System;
using WHRID.Domain.Enums;

namespace WHRID.Domain.Entities
{
    public class RiskProfile
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public Wallet? Wallet { get; set; }

        public double RiskScore { get; set; } // 0-100
        public double ActivityScore { get; set; } // 0-100
        public double TokenHealthScore { get; set; } // 0-100

        public WalletClassification Classification { get; set; }
        public DateTime CalculatedAt { get; set; }
    }
}
