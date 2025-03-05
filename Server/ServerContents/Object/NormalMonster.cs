using Google.Protobuf.Protocol;
using ServerContents.Room;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

enum NormalMonsterState
{
    Idle = 0,
    Move = 1,
    Stun = 2,
    Skill = 3,
    Dead = 4,
}

namespace ServerContents.Object
{
    class NormalMonster : Monster
    {
        NormalMonsterState _currentState = NormalMonsterState.Idle;

        public NormalMonster()
        {
            ObjectType = GameObjectType.Normalmonster;

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
                case NormalMonsterState.Idle:
                    UpdateIdle();
                    break;
                case NormalMonsterState.Move:
                    UpdateMove();
                    break;
                case NormalMonsterState.Stun:
                    UpdateStun();
                    break;
                case NormalMonsterState.Skill:
                    UpdateSkill();
                    break;
                case NormalMonsterState.Dead:
                    UpdateDead();
                    break;
            }

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
            // TODO: 일반 몬스터는 아직 스킬 구현 계획 없음. 추후 확장성을 위함
        }

        protected override void UpdateThink()
        {
            if (_currentState == NormalMonsterState.Dead) return;

            // 현재 시간을 바탕으로 경과 시간 계산
            DateTime currentTime = DateTime.Now;
            float elapsedTime = (float)(currentTime - _lastThinkTime).TotalSeconds;

            // 스턴 상태일 경우
            if (_currentState == NormalMonsterState.Stun)
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
            else if (_currentState == NormalMonsterState.Skill)
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

        protected override void Think()
        {
            // 타겟이 설정되지 않은 경우 Idle, Move 중 random Selection
            if (_target == null)
            {
                _currentState = (NormalMonsterState)rnd.Next(0, 2);

                // 방향 랜덤 Selection
                _isRight = rnd.Next(0, 2) == 0;
            }
            // 타겟이 설정된 경우 Move, Skill 중 random Selection
            else
            {
                _currentState = NormalMonsterState.Move;

                // TODO: 일반 몬스터는 아직 스킬 구현 계획 없음. 추후 확장성을 위함
                // int random = rnd.Next(0, 2);
                // if (random == 0) _currentState = NormalMonsterState.Move;
                // else UpdateSkill();
            }
        }

        protected override void UpdateIdle()
        {
            _currentState = NormalMonsterState.Idle;

            // 위치 변동 없음
        }

        protected override void UpdateMove()
        {
            _currentState = NormalMonsterState.Move;

            _destinationPos.X += (_isRight ? Stat.Speed : -Stat.Speed);

            if (_target != null)
                _isRight = _target.Info.PositionX > _destinationPos.X ? true : false;

            _destinationPos.X = Math.Clamp(_destinationPos.X, _minX, _maxX);
        }

        protected override void UpdateStun()
        {
            if (_currentState != NormalMonsterState.Stun)
                _stunStartTime = DateTime.Now; // 스턴 상태로 전환할 때 시작 시간 기록.

            _currentState = NormalMonsterState.Stun;

            // 위치 변동 없음.
        }

        protected override void UpdateSkill()
        {
            // TODO: 일반 몬스터는 아직 개발 계획 없음.
            if (_currentState != NormalMonsterState.Skill)
            {
                _skillStartTime = DateTime.Now; // 스킬 상태로 전환할 때 시작 시간 기록.
                BroadcastSkill();
            }
                

            _currentState = NormalMonsterState.Skill;

            // 위치 변동 없음.
        }

        protected override void UpdateDead()
        {
            _currentState = NormalMonsterState.Dead;

            // 위치 변동 없음
        }

        public override void TakeDamage(int attackPower)
        {
            base.TakeDamage(attackPower);

            if (Stat.Hp > 0)
            {
                UpdateStun();

                // 다른 플레이어에게도 알린다.
                S_HitMonster hitPacket = new S_HitMonster();
                hitPacket.MonsterId = Id;
                Room.Broadcast(hitPacket);
            }
            else if (Stat.Hp <= 0)
            {
                UpdateDead();

                // 다른 플레이어에게도 알린다.
                S_MonsterDespawn despawnPacket = new S_MonsterDespawn();
                despawnPacket.MonsterIds.Add(Id);
                Room.Broadcast(despawnPacket);

                Room.LeaveMonster(Id);
                MonsterManager.Instance.MonsterDespawn(Id);
            }
        }

        public override void SetTarget(Player newTarget)
        {
            _target = newTarget;
        }
    }
}