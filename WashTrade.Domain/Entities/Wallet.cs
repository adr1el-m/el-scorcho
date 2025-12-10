using System.Collections.Generic;

namespace WashTrade.Domain.Entities
{
    public class Wallet
    {
        public int Id { get; set; }
        public string Address { get; set; } = string.Empty;

        // Navigation properties
        public ICollection<Transaction> SentTransactions { get; set; } = new List<Transaction>();
        public ICollection<Transaction> ReceivedTransactions { get; set; } = new List<Transaction>();
        public ICollection<WashTradeAlert> Alerts { get; set; } = new List<WashTradeAlert>();
    }
}
