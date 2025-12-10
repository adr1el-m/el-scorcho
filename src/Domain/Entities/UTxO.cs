using System;

namespace WHRID.Domain.Entities
{
    public class UTxO
    {
        public string TxHash { get; set; } = string.Empty;
        public int Index { get; set; }
        public ulong LovelaceAmount { get; set; }
        
        public Guid WalletId { get; set; }
        public Wallet? Wallet { get; set; }
    }
}
