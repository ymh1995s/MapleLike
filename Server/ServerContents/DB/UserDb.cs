using Google.Protobuf.Protocol;
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
        public string DbId { get; set; } // 편의상 string으로 만들었는데 속도면에서 int가 유리

        public int Level { get; set; }
        public ClassType ClassType { get; set; }

        public int MapNo { get; set; }

        public int CurrentExp { get; set; }
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

        public int STR { get; set; }
        public int DEX { get; set; }
        public int INT { get; set; }
        public int LUK { get; set; }

        public int Gold { get; set; }

        public virtual ICollection<InventoryDb> Inventory { get; set; } = new List<InventoryDb>();
    }
}
