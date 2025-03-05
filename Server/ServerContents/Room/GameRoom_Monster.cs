using Google.Protobuf.Protocol;
using ServerContents.Job;
using ServerContents.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.Room
{
    // 몬스터와 관련된 로직을 관리한다.
    // 기환님 작업하는 곳.
    public partial class GameRoom : JobSerializer
    {
        // TEMP 스폰포인트 관리 
        // 이것좀 기환님 스타일대로 관리 부탁합니다!
        List<PositionInfo> playerSpawnPoints = new List<PositionInfo>();

        struct PositionInfo
        {
            public float x;
            public float y;
        }

        void SetRoomData(int roomId/*ID는 혹시 몰라서 일단 파놓음*/)
        {
            // TEMP 플레이거 처음 시작했을 때 스폰되는 포인트를 하드코딩으로 테스트
            // 이것좀 기환님 스타일대로 관리 부탁합니다!
            PositionInfo positionInfo = new PositionInfo();
            positionInfo.x = 0f;
            positionInfo.y = -15f;
            playerSpawnPoints.Add(positionInfo);
        }

        void MonsterUpdate()
        {
            MonsterManager.Instance.MonsterSpawn(RoomId);

            foreach (NormalMonster nomralMonster in _normalMonsters.Values)
            {
                nomralMonster.Update();
            }

            foreach (BossMonster bossMonster in _bossMonsters.Values)
            {
                bossMonster.Update();
            }
        }

        public void MonsterEnterGame(GameObject gameObject)
        {
            lock (_lock)
            {
                if (gameObject == null)
                    return;

                GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

                if (type == GameObjectType.Normalmonster)
                {
                    NormalMonster monster = gameObject as NormalMonster;
                    _normalMonsters.Add(gameObject.Id, monster);
                    monster.Room = this;
                }
                else if (type == GameObjectType.Bossmonster)
                {
                    BossMonster monster = gameObject as BossMonster;
                    _bossMonsters.Add(gameObject.Id, monster);
                    monster.Room = this;
                }

                // 게임룸에 입장한 사실을 다른 클라이언트에게 전송
                {
                    S_MonsterSpawn spawnPacket = new S_MonsterSpawn();
                    spawnPacket.MonsterInfos.Add((gameObject as Monster)?.Info);
                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != gameObject.Id)
                            p.Session.Send(spawnPacket);
                    }
                }
            }
        }

        // 몬스터를 관리 대상에서(딕셔너리) 삭제
        public void LeaveMonster(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if (type == GameObjectType.Normalmonster)
            {
                NormalMonster monster = null;
                if (_normalMonsters.Remove(objectId, out monster) == false)
                    return;

                monster.Room = null;
            }
            else if (type == GameObjectType.Bossmonster)
            {
                BossMonster projectile = null;
                if (_bossMonsters.Remove(objectId, out projectile) == false)
                    return;

                projectile.Room = null;
            }

            // 타인한테 정보 전송
            {
                // TODO : 디스폰 구분
                S_MonsterDespawn despawnPacket = new S_MonsterDespawn();
                despawnPacket.MonsterIds.Add(objectId);
                foreach (Player p in _players.Values)
                {
                    if (p.Id != objectId)
                        p.Session.Send(despawnPacket);
                }
            }
        }

        /// <summary>
        /// 몬스터가 플레이러를 타게팅한다.
        /// 재사용성에 대해서는 검토가 필요할 듯
        /// </summary>
        /// <param name="타게팅할 플레이어 객체입니다."></param>
        /// <param name="타게팅할 몬스터의 ID입니다. 이 ID로 딕셔너리에서 찾아줄겁니다."></param>
        public void MonsterSetTargetToPlayer(Player player, int monsterId)
        {
            // 용도를 Hit 처리와 타겟 세팅을 함께 처리해도 될 듯.
            // 만약 죽었다면 Monster에서 같은 룸에 있는 클라이언트에게 뿌릴꺼니까
            Monster monster = null;

            if (_normalMonsters.TryGetValue(monsterId, out var normalMonster))
            {
                monster = normalMonster;
            }
            else if (_bossMonsters.TryGetValue(monsterId, out var bossMonster))
            {
                monster = bossMonster;
            }

            if (monster != null)
            {
                monster.SetTarget(player);
                monster.TakeDamage(10);
            }
            else
            {
                Console.WriteLine("YMH : 몬스터 타게팅 실패. 해당 몬스터를 찾지 못했습니다(서버 로직 오류)");
            }
        }

        // TODO: 추후, C_HitPacket에 playerAttackPower가 들어오면 MonsterSetTargetToPlayer 함수를 해당 함수로 대체
        //public void MonsterHitSetTarget(Player player, int monsterId, int damageAmount)
        //{
        //    Monster monster = null;

        //    if (_normalMonsters.TryGetValue(monsterId, out var normalMonster))
        //        monster = normalMonster;

        //    else if (_bossMonsters.TryGetValue(monsterId, out var bossMonster))
        //        monster = bossMonster;


        //    if (monster != null)
        //    {
        //        monster._target = player;
        //        monster.TakeDamage(10);
        //    }
        //    else
        //    {
        //        Console.WriteLine("YMH : 몬스터 타게팅 실패. 해당 몬스터를 찾지 못했습니다(서버 로직 오류)");
        //    }
        //}
    }
}
