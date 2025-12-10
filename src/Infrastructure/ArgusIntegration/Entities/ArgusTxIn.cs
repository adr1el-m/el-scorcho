using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WHRID.Infrastructure.ArgusIntegration.Entities
{
    [Table("tx_in")]
    public class ArgusTxIn
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("tx_in_id")]
        public long TxInId { get; set; } // The transaction that consumes this input

        [Column("tx_out_id")]
        public long TxOutId { get; set; } // The transaction output being consumed

        [Column("tx_out_index")]
        public int TxOutIndex { get; set; }

        [ForeignKey("TxInId")]
        public required ArgusTransaction ConsumingTransaction { get; set; }
    }
}
