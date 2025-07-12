using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.DB
{
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Etc
    }

    [Table("Items")]
    public class ItemDb
    {
        [Key]
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
