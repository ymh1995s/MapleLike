using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;

namespace ServerContents.DB
{
    [Table("ItemConsumables")]
    public class ItemConsumableDb
    {
        [Key, ForeignKey("Item")]
        public ItemType ItemDbId { get; set; } // PK & FK

        public int HealHp { get; set; }
        public int HealMp { get; set; }

        public ItemDb Item { get; set; }
    }
}
