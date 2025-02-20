using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.Object
{
    class BossMonster : Monster
    {
        Player _target;

        public BossMonster()
        {
            ObjectType = GameObjectType.Bossmonster;

            //// TEMP
            //Stat.Level = 1;
            //Stat.Hp = 100;
            //Stat.MaxHp = 100;
            //Stat.Speed = 5.0f;

            //State = CreatureState.Idle;
        }

        public override void Update()
        {
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

        protected virtual void UpdateDead()
        {

        }
    }
}
