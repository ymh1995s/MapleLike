using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.DB
{
    [Table("ItemEquips")]
    public class ItemEquipDb
    {
        [Key, ForeignKey("Item")]
        public int ItemDbId { get; set; } // PK & FK

        public string RequiredJob { get; set; }

        public int BonusAttack { get; set; }
        public int BonusDefense { get; set; }

        public ItemDb Item { get; set; }
    }
}
