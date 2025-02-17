using Google.Protobuf.Protocol;
using ServerContents.Object;
using ServerContents.Session;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerContents.Object
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }

        public Player()
        {
            ObjectType = GameObjectType.Player;
        }

        public override void OnDamaged(GameObject attacker, int damage)
        {
            base.OnDamaged(attacker, damage);
        }

        public override void OnDead(GameObject attacker)
        {
            base.OnDead(attacker);
        }
    }
}
