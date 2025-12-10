using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WHRID.Infrastructure.ArgusIntegration.Entities
{
    [Table("tx")]
    public class ArgusTransaction
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("hash")]
        public required byte[] Hash { get; set; }

        [Column("block_id")]
        public long BlockId { get; set; }

        [Column("block_index")]
        public int BlockIndex { get; set; }

        [Column("fee")]
        public decimal Fee { get; set; }
        
        [ForeignKey("BlockId")]
        public required ArgusBlock Block { get; set; }

        public ICollection<ArgusTxOut> Outputs { get; set; } = new List<ArgusTxOut>();
        public ICollection<ArgusTxIn> Inputs { get; set; } = new List<ArgusTxIn>();
    }
}
