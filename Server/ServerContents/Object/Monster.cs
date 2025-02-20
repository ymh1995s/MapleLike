using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.Object
{
    class Monster : GameObject
    {
        public MonsterInfo Info { get; set; } = new MonsterInfo();
        public MonsterStatInfo Stat { get; private set; } = new MonsterStatInfo();

        public float Speed
        {
            get { return Stat.Speed; }
            set { Stat.Speed = value; }
        }

        public int Hp
        {
            get { return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0, Stat.Hp); }
        }

        Player _target;

        public Monster()
        {
            Info.StatInfo = Stat;
        }

        public override void Update()
        {

        }

        Random random = new Random();

        protected void BroadcastMove()
        {
            // 다른 플레이어한테도 알려준다
            // TODO : Normal 몬스터의 부모 클래스를 구분한다.
            S_MonsterMove movePacket = new S_MonsterMove();
            movePacket.MonsterId = Id;
            movePacket.DestinationX = Info.DestinationX + (float)random.NextDouble();
            Room.Broadcast(movePacket);
        }
    }
}
