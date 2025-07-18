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
    [Table("Inventories")]
    public class InventoryDb
    {
        [Key]
        public int InventoryDbId { get; set; }

        [ForeignKey("Item")]
        public ItemType ItemDbId { get; set; }

        [ForeignKey("User")]
        public string UserDbId { get; set; }

        public int Count { get; set; }
        public int MaxCount { get; set; }
        public ItemState IsEquipped { get; set; }

        public ItemDb Item { get; set; }
        public UserDb User { get; set; }
    }
}
