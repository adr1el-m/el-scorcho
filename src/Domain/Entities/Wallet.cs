using System;
using System.Collections.Generic;

namespace WHRID.Domain.Entities
{
    public class Wallet
    {
        public Guid Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public DateTime FirstSeen { get; set; }
        public DateTime LastActive { get; set; }
        
        public RiskProfile? RiskProfile { get; set; }
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<UTxO> UTxOs { get; set; } = new List<UTxO>();
    }
}
