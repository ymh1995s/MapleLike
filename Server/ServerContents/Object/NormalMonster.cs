using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.Object
{
    class NormalMonster : GameObject
    {
        Player _target;

        public NormalMonster()
        {
            ObjectType = GameObjectType.Normalmonster;

            //// TEMP
            //Stat.Level = 1;
            //Stat.Hp = 100;
            //Stat.MaxHp = 100;
            //Stat.Speed = 5.0f;

            //State = CreatureState.Idle;
        }

        public override void Update()
        {
            BroadcastMove();
            // FSM을 생각중이지만 몬스터 담당자와 협의
            //switch (State)
            //{
            //    case CreatureState.Idle:
            //        UpdateIdle();
            //        break;
            //    case CreatureState.Moving:
            //        UpdateMoving();
            //        break;
            //    case CreatureState.Skill:
            //        UpdateSkill();
            //        break;
            //    case CreatureState.Dead:
            //        UpdateDead();
            //        break;
            //}
        }

        Random random= new Random();
        void BroadcastMove()
        {
            // 다른 플레이어한테도 알려준다
            S_Move movePacket = new S_Move();
            movePacket.ObjectId = Id;
            movePacket.PosX = Info.PosX + (float)random.NextDouble();
            Room.Broadcast(movePacket);
        }

        protected virtual void UpdateDead()
        {

        }
    }
}
