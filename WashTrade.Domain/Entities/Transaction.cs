using System;

namespace WashTrade.Domain.Entities
{
    public class Transaction
    {
        public long Id { get; set; }
        public string TxHash { get; set; } = string.Empty;
        public long BlockHeight { get; set; }
        public DateTime Timestamp { get; set; }

        public int? SenderWalletId { get; set; }
        public Wallet? SenderWallet { get; set; }

        public int? ReceiverWalletId { get; set; }
        public Wallet? ReceiverWallet { get; set; }

        public int? AssetId { get; set; }
        public Asset? Asset { get; set; }

        public decimal Amount { get; set; } // Quantity of the asset
    }
}
