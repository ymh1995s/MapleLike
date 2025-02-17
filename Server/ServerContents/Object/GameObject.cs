using Google.Protobuf.Protocol;
using ServerContents.Room;
using ServerContents.Session;

namespace ServerContents.Object
{
    // GameObject : 서버와 관련된 모든 게임 오브젝트
    // ex. 플레이어, 몬스터, 아이템 등 동기화가 필요한 오브젝트
    public class GameObject
    {
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;

        public int Id
        {
            get { return Info.ObjectId; }
            set { Info.ObjectId = value; }
        }

        public GameRoom Room { get; set; }
        public ObjectInfo Info { get; set; } = new ObjectInfo();
        public StatInfo Stat { get; private set; } = new StatInfo();

        public float Speed
        {
            get { return Stat.Speed; }
            set { Stat.Speed = value; }
        }

        public int Hp
        {
            get { return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); }
        }

        public GameObject()
        {
            //Info.PosInfo = PosInfo;
            Info.StatInfo = Stat;
        }

        public virtual void Update()
        {

        }

        public virtual void OnDamaged(GameObject attacker, int damage)
        {
            if (Room == null)
                return;

            Stat.Hp = Math.Max(Stat.Hp - damage, 0);

            S_ChangeHp changePacket = new S_ChangeHp();
            changePacket.ObjectId = Id;
            changePacket.Hp = Stat.Hp;
            Room.Broadcast(changePacket);

            if (Stat.Hp <= 0)
            {
                OnDead(attacker);
            }
        }

        public virtual void OnDead(GameObject attacker)
        {
            if (Room == null)
                return;

            S_Die diePacket = new S_Die();
            diePacket.ObjectId = Id;
            diePacket.AttackerId = attacker.Id;
            Room.Broadcast(diePacket);

            GameRoom room = Room;
            room.LeaveGame(Id);

            // TODO 플레이어일 시 스텟과 맵 위치 등 조정
            // TODO 몬스터일 때 그냥 바로 부활시킬까? => 어쨋든 너도 하위 클래스에서 

            //room.EnterGame(this); => 플레이어일 떄 (하위클래스에서 정의)
        }
    }
}
