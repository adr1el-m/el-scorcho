using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WHRID.Infrastructure.ArgusIntegration.Entities
{
    [Table("block")]
    public class ArgusBlock
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("hash")]
        public required byte[] Hash { get; set; }

        [Column("epoch_no")]
        public int? EpochNo { get; set; }

        [Column("slot_no")]
        public long? SlotNo { get; set; }

        [Column("block_no")]
        public long? BlockNo { get; set; }

        [Column("time")]
        public DateTime Time { get; set; }
    }
}
