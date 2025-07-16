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
    [Table("Items")]
    public class ItemDb
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)] // 키 값을 수동으로 변경할 수 있게 설정
        public int ItemDbId { get; set; } // PK

        public string Name { get; set; }
        public ItemType Type { get; set; }

        public string Description { get; set; }

        public int SellPrice { get; set; }
        public int BuyPrice { get; set; }

        public ItemEquipDb ItemEquip { get; set; }
        public ItemConsumableDb ItemConsumable { get; set; }

        public ICollection<InventoryDb> Inventory { get; set; }
    }
}
