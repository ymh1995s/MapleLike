using Google.Protobuf.Protocol;
using ServerContents.Room;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

enum BossMonsterState
{
    Idle = 0,
    Move = 1,
    Stun = 2,
    Skill = 3,
    Dead = 4,
}

namespace ServerContents.Object
{
    class BossMonster : Monster
    {
        BossMonsterState _currentState = BossMonsterState.Idle;

        private DateTime _lastSetTargetTime = DateTime.Now;

        public BossMonster()
        {
            ObjectType = GameObjectType.Bossmonster;

            _lastThinkTime = DateTime.Now;
            _thinkInterval = 0.0f;
        }

        public override void Update()
        {
            // 클라이언트와 주고 받은 패킷을 기반으로 State 결정
            UpdateThink();
            UpdateTarget();

            switch (_currentState)
            {
                case BossMonsterState.Idle:
                    UpdateIdle();
                    break;
                case BossMonsterState.Move:
                    UpdateMove();
                    break;
                case BossMonsterState.Stun:
                    UpdateStun();
                    break;
                case BossMonsterState.Skill:
                    UpdateSkill();
                    break;
                case BossMonsterState.Dead:
                    UpdateDead();
                    break;
            }

            UpdateInfo();
            BroadcastMove();
        }

        protected override void BroadcastMove()
        {
            // 다른 플레이어한테도 알려준다.
            S_MonsterMove movePacket = new S_MonsterMove();
            movePacket.State = (MonsterState)_currentState;
            movePacket.MonsterId = Id;
            movePacket.DestinationX = Math.Clamp(_destinationPos.X, _minX, _maxX);
            movePacket.DestinationY = _destinationPos.Y;
            movePacket.IsRight = _isRight;

            Room.Broadcast(movePacket);
        }

        protected override void BroadcastSkill()
        {
            S_MonsterSkill monsterSkillPacket = new S_MonsterSkill();
            monsterSkillPacket.MonsterId = Id;
            monsterSkillPacket.SkillType = (BossMonsterSkillType)(rnd.Next(0, 2));  // 현재 구현은 2개의 스킬이므로 둘 중에 하나 랜덤 선택

            Room.Broadcast(monsterSkillPacket);
        }

        protected override void UpdateInfo()
        {
            // Update Info and Stat
            Info.DestinationX = Math.Clamp(_destinationPos.X, _minX, _maxX);
            Info.DestinationY = _destinationPos.Y;
            Info.CreatureState = (MonsterState)_currentState;
            Info.MonsterId = Id;
            Info.StatInfo = Stat;
        }

        protected override void UpdateThink()
        {
            if (_currentState == BossMonsterState.Dead) return;

            // 현재 시간을 바탕으로 경과 시간 계산
            DateTime currentTime = DateTime.Now;
            float elapsedTime = (float)(currentTime - _lastThinkTime).TotalSeconds;

            // 스턴 상태일 경우
            if (_currentState == BossMonsterState.Stun)
            {
                // 스턴 지속 시간이 경과했는지 확인
                if ((float)(currentTime - _stunStartTime).TotalSeconds >= _stunDuration)
                {
                    // 스턴이 끝난 후 Think를 호출
                    Think();

                    _thinkInterval = (float)(rnd.NextDouble() * 2 + 3); // 3초에서 5초
                    _lastThinkTime = currentTime;
                }
            }
            else if (_currentState == BossMonsterState.Skill)
            {
                if ((float)(currentTime - _skillStartTime).TotalSeconds >= _skillDuration)
                {
                    Think();

                    _thinkInterval = (float)(rnd.NextDouble() * 2 + 3); // 3초에서 5초
                    _lastThinkTime = currentTime;
                }
            }

            else // 스턴 상태, 스킬 사용 상태가 아닐 경우
            {
                if (elapsedTime >= _thinkInterval)
                {
                    Think();

                    _thinkInterval = (float)(rnd.NextDouble() * 2 + 3); // 3초에서 5초
                    _lastThinkTime = currentTime;
                }
            }
        }

        // Move, Skill random Selection
        protected override void Think()
        {
            int random = rnd.Next(0, 2);
            if (random == 0)
            {
                if (_target == null)
                {
                    _currentState = (BossMonsterState)rnd.Next(0, 2);

                    _isRight = rnd.Next(0, 2) == 0;
                }
                else _currentState = BossMonsterState.Move;

            }
            else UpdateSkill();
        }

        protected override void UpdateIdle()
        {
            _currentState = BossMonsterState.Idle;

            // 위치 변동 없음
        }

        protected override void UpdateMove()
        {
            _currentState = BossMonsterState.Move;

            _destinationPos.X += (_isRight ? Stat.Speed : -Stat.Speed);

            if (_target != null)
                _isRight = _target.Info.PositionX > _destinationPos.X ? true : false;

            _destinationPos.X = Math.Clamp(_destinationPos.X, _minX, _maxX);
        }

        // TODO: 보스가 스턴이 있는가?
        protected override void UpdateStun()
        {
            if (_currentState != BossMonsterState.Stun)
                _stunStartTime = DateTime.Now; // 스턴 상태로 전환할 때 시작 시간 기록

            _currentState = BossMonsterState.Stun;

            // 위치 변동 없음
        }

        protected override void UpdateSkill()
        {
            if (_currentState != BossMonsterState.Skill)
            {
                _skillStartTime = DateTime.Now; // 스킬 상태로 전환할 때 시작 시간 기록.
                BroadcastSkill();
            }

            _currentState = BossMonsterState.Skill;
        }

        protected override void UpdateDead()
        {
            _currentState = BossMonsterState.Dead;

            // 위치 변동 없음
        }

        public override void TakeDamage(int playerId, int attackPower)
        {
            base.TakeDamage(playerId, attackPower);

            if (Stat.Hp > 0)
            {
                S_HitMonster hitPacket = new S_HitMonster();
                hitPacket.MonsterId = Id;
                hitPacket.MonsterCurrentHp = Stat.Hp;
                Room.Broadcast(hitPacket);
            }
            else if (Stat.Hp <= 0)
            {
                UpdateDead();

                // TODO: 경험치 제공
                // TODO: 아이템 제공
               
                // 현재 룸에 존재하는 모든 클라이언트에게 알림
                Room.LeaveMonster(Id);

                MonsterManager.Instance.MonsterDespawn(Id);

                Room.GameClear();
            }
        }

        public override void SetTarget(Player newTarget)
        {
            DateTime currentTime = DateTime.Now;

            if ((currentTime - _lastSetTargetTime).TotalSeconds <= 10.0f)
                return;

            _target = newTarget;
            _lastSetTargetTime = currentTime;
        }
    }
}

