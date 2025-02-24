using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerContents.Job;
using ServerContents.Object;
using ServerContents.Session;
using ServerCore;
using System;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

// 메이플스토리의 1개의 맵이라고 보면 됩니다.
namespace ServerContents.Room
{
    // 컨텐츠 로직을 게임에서 관리한다.
    public class GameRoom : JobSerializer
    {
        public int RoomId { get; set; }

        public int recvPacketCount = 0;
        public int sendPacketCount = 0;

        // 게임룸과 연결된 모든 오브젝트를 딕셔너리로 관리 (ObjectManager에서 생성된 객체 중 이 GameRoom에 있는 객체 관리)
        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, NormalMonster> _normalMonsters = new Dictionary<int, NormalMonster>();
        Dictionary<int, BossMonster> _bossMonsters = new Dictionary<int, BossMonster>();
        // Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>(); 후순위
        // Dictionary<int, Item> _projectiles = new Dictionary<int, Item>(); 후순위

        // 패킷 모아보내기용 List
        List<IMessage> _pendingList = new List<IMessage>();

        Random random = new Random();

        public GameRoom()
        {
#if DEBUG
            PrintProceecPacket();
#endif
        }

        public void Init()
        {
            // Nms초당 에 한 번 서버에 귀속적인 로직을 실행한다.
            // ex. 몬스터의 업데이트
            Update(100);

            // temp 몬스터 입장 
            NormalMonster monster = ObjectManager.Instance.Add<NormalMonster>();
            monster.Info.Name = $"Monster_{monster.Info.Name}_{monster.Info.MonsterId}";
            MonsterEnterGame(monster);
        }

        void Update(int waitms)
        {
            foreach (NormalMonster nomralMonster in _normalMonsters.Values)
            {
                nomralMonster.Update();
            }

            foreach (BossMonster bossMonster in _bossMonsters.Values)
            {
                bossMonster.Update();
            }

            PushAfter(waitms, Update, waitms);
        }

        object _lock = new object();
        public void PlayerEnterGame(GameObject gameObject)
        {
            lock (_lock)
            {
                if (gameObject == null)
                    return;

                GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

                Player player = gameObject as Player;
                _players.Add(gameObject.Id, player);
                player.Room = this;

                // 게임룸에 있는 모든 객체를 입장한 본인에게 전송
                {
                    // S_Enter : 자기 자신의 캐릭터 
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.PlayerInfo = player.Info;
                    player.Session.Send(enterPacket);

                    // S_Spawn : 다른 사람의 캐릭터
                    S_PlayerSpawn playersSpawnPacket = new S_PlayerSpawn();
                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
                            playersSpawnPacket.PlayerInfos.Add(p.Info);
                    }

                    // 현재 있는 map의 몬스터 동기화
                    S_MonsterSpawn monstersSpawnPacket = new S_MonsterSpawn();
                    foreach (NormalMonster m in _normalMonsters.Values)
                        monstersSpawnPacket.MonsterInfos.Add(m.Info);
                    foreach (BossMonster m in _bossMonsters.Values)
                        monstersSpawnPacket.MonsterInfos.Add(m.Info);

                    player.Session.Send(playersSpawnPacket);
                    player.Session.Send(monstersSpawnPacket);
                }

                // 게임룸에 입장한 사실을 다른 클라이언트에게 전송
                {
                    S_PlayerSpawn spawnPacket = new S_PlayerSpawn(); 
                    spawnPacket.PlayerInfos.Add((gameObject as Player)?.Info);
                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != gameObject.Id)
                            p.Session.Send(spawnPacket);
                    }
                }
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

        // 플레이어를 관리 대상에서(딕셔너리) 삭제
        public void LeavePlayer(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;

                player.Room = null;

                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }

            // 타인한테 정보 전송
            {
                S_PlayerDespawn despawnPacket = new S_PlayerDespawn();
                despawnPacket.PlayerIds.Add(objectId);
                foreach (Player p in _players.Values)
                {
                    if (p.Id != objectId)
                        p.Session.Send(despawnPacket);
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

        public void Flush()
        {
            if (_pendingList.Count == 0) return;
            foreach (var s in _players)
            {
                SendPacketPlus();
                s.Value.Session.Send(_pendingList);
            }
            _pendingList.Clear();
        }

        // 플레이어의 위치 동기화 
        public void HandleMove(Player player, C_PlayerMove movePacket)
        {
            if (player == null)
                return;

            // 서버에서 관리하기 위해 데이터 반영
            _players[player.Info.PlayerId].Info.PositionX = movePacket.PositionX;
            _players[player.Info.PlayerId].Info.PositionY = movePacket.PositionY;

            // 다른 플레이어한테도 알려준다
            S_PlayerMove resMovePacket = new S_PlayerMove();
            resMovePacket.PlayerId = player.Info.PlayerId;
            resMovePacket.PositionX = movePacket.PositionX;
            resMovePacket.PositionY = movePacket.PositionY;
            RecvPacketPlus();
            Broadcast(resMovePacket);
        }

        public void HandleSkill(Player player, C_PlayerSkill skillPacket)
        {
            // TODO 패킷 구조 설계 후 이어서 

            //if (player == null)
            //    return;

            //ObjectInfo info = player.Info;
            //if (info.PosInfo.State != CreatureState.Idle)
            //    return;

            //// TODO : 스킬 사용 가능 여부 체크
            //info.PosInfo.State = CreatureState.Skill;
            //S_Skill skill = new S_Skill() { Info = new SkillInfo() };
            //skill.ObjectId = info.ObjectId;
            //skill.Info.SkillId = skillPacket.Info.SkillId;
            //Broadcast(skill);

            //Data.Skill skillData = null;
            //if (DataManager.SkillDict.TryGetValue(skillPacket.Info.SkillId, out skillData) == false)
            //    return;

            //switch (skillData.skillType)
            //{
            //    case SkillType.SkillAuto:
            //        {
            //            Vector2Int skillPos = player.GetFrontCellPos(info.PosInfo.MoveDir);
            //            GameObject target = Map.Find(skillPos);
            //            if (target != null)
            //            {
            //                Console.WriteLine("Hit GameObject !");
            //            }
            //        }
            //        break;
            //    case SkillType.SkillProjectile:
            //        {
            //            Arrow arrow = ObjectManager.Instance.Add<Arrow>();
            //            if (arrow == null)
            //                return;

            //            arrow.Owner = player;
            //            arrow.Data = skillData;
            //            arrow.PosInfo.State = CreatureState.Moving;
            //            arrow.PosInfo.MoveDir = player.PosInfo.MoveDir;
            //            arrow.PosInfo.PosX = player.PosInfo.PosX;
            //            arrow.PosInfo.PosY = player.PosInfo.PosY;
            //            arrow.Speed = skillData.projectile.speed;
            //            Push(EnterGame, arrow);
            //        }
            //        break;
            //}
        }

        // 얘는 필요한가? 보류
        //public void HandleDie(GameObject go, C_Die diePacket)
        //{
        //    if (go == null)
        //        return;

        //    LeaveGame(go.Info.ObjectId);

        //    go.Info.PosInfo.CurrentPosX = 0;
        //    go.Info.PosInfo.CurrentPosY = 0;
        //    go.Info.PosInfo.CurrentPosZ = 0;
        //    go.Info.PosInfo.DestinationPosX = 0;
        //    go.Info.PosInfo.DestinationPosY = 0;
        //    go.Info.PosInfo.DestinationPosZ = 0;

        //    EnterGame(go);
        //}

        /// <summary>
        /// 몬스터가 플레이러를 타게팅한다.
        /// 재사용성에 대해서는 검토가 필요할 듯
        /// </summary>
        /// <param name="타게팅할 플레이어 객체입니다."></param>
        /// <param name="타게팅할 몬스터의 ID입니다. 이 ID로 딕셔너리에서 찾아줄겁니다."></param>
        public void MonsterSetTargetToPlayer(Player player, int monsterId)
        {
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
                // TO 기환. 이거좀 프로퍼티 같은걸로 뚫어주세요.
                // monster._target = player;
            }
            else
            {
                Console.WriteLine("YMH : 몬스터 타게팅 실패. 해당 몬스터를 찾지 못했습니다(서버 로직 오류)");
            }
        }

        // 게임룸에 있는 다른 클라이언트에게 알림
        public void Broadcast(IMessage packet)
        {
            _pendingList.Add(packet);
        }

        #region 패킷 송수신 개수 개략적 확인용
        public void RecvPacketPlus()
        {
            Interlocked.Increment(ref recvPacketCount);
            //recvPacketCount++;
        }

        public void SendPacketPlus()
        {
            Interlocked.Increment(ref sendPacketCount);
            //sendPacketCount++;
        }

        private async void PrintProceecPacket()
        {
            while (true)
            {
                Console.WriteLine($"{RoomId}번 방에서 총{recvPacketCount + sendPacketCount}, recv : {recvPacketCount}개 / send : {sendPacketCount}개을 1초에 처리");

                Interlocked.Exchange(ref recvPacketCount, 0);
                Interlocked.Exchange(ref sendPacketCount, 0);
                //recvPacketCount = 0;
                //sendPacketCount = 0;

                await Task.Delay(1000); // 1초 대기 (비동기적으로 실행)
            }
        }

        #endregion 패킷 송수신 개수 개략적 확인용
    }
}
