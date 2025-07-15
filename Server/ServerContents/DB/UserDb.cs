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
        public string UserDbId { get; set; } // 편의상 string으로 만들었는데 속도면에서 int가 유리

        public int Level { get; set; }
        public int ClassType { get; set; }

        public int MapNo { get; set; }

        public int Exp { get; set; }
        public int MaxExp { get; set; }

        public int CurrentHp { get; set; }
        public int MaxHp { get; set; }

        public int CurrentMp { get; set; }
        public int MaxMp { get; set; }

        public int AttackPower { get; set; }
        public int MagicPower { get; set; }
        public int Defense { get; set; }

        public float Speed { get; set; }
        public float Jump { get; set; }

        public float STR { get; set; }
        public float DEX { get; set; }
        public float INT { get; set; }
        public float LUK { get; set; }

        public int Gold { get; set; }

        public ICollection<InventoryDb> Inventory { get; set; }
    }
}
