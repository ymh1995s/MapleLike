using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.DB
{
    [Table("Users")]
    public class UserDb
    {
        [Key]
        public int UserDbId { get; set; } // PK

        public int Level { get; set; }
        public string Job { get; set; }

        public int MapNo { get; set; }

        public int Exp { get; set; }
        public int MaxExp { get; set; }

        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }

        public int CurrentMp { get; set; }
        public int MaxMp { get; set; }

        public int Attack { get; set; }
        public int Defense { get; set; }

        public float MoveSpeed { get; set; }
        public float JumpPower { get; set; }

        public int Gold { get; set; }

        public ICollection<InventoryDb> Inventory { get; set; }
    }
}
