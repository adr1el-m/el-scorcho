using System.Collections.Generic;

namespace WashTrade.Domain.Entities
{
    public class Asset
    {
        public int Id { get; set; }
        public string PolicyId { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty; // Hex encoded usually
        public string Fingerprint { get; set; } = string.Empty; // CIP-14

        // Navigation properties
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
