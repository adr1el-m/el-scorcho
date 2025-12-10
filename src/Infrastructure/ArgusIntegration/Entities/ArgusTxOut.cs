using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WHRID.Infrastructure.ArgusIntegration.Entities
{
    [Table("tx_out")]
    public class ArgusTxOut
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("tx_id")]
        public long TxId { get; set; }

        [Column("index")]
        public int Index { get; set; }

        [Column("address")]
        public required string Address { get; set; }

        [Column("value")]
        public decimal Value { get; set; }

        [Column("data_hash")]
        public byte[]? DataHash { get; set; }

        [ForeignKey("TxId")]
        public required ArgusTransaction Transaction { get; set; }
    }
}
