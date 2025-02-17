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
            EnterGame(monster);
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
        public void EnterGame(GameObject gameObject)
        {
            lock (_lock)
            {
                if (gameObject == null)
                    return;

                GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

                if (type == GameObjectType.Player)
                {
                    Player player = gameObject as Player;
                    _players.Add(gameObject.Id, player);
                    player.Room = this;

                    // 게임룸에 있는 모든 객체를 입장한 본인에게 전송
                    {
                        // S_Enter : 자기 자신의 캐릭터 
                        S_EnterGame enterPacket = new S_EnterGame();
                        enterPacket.Player = player.Info;
                        player.Session.Send(enterPacket);

                        // S_Spawn : 다른 사람의 캐릭터
                        S_Spawn spawnPacket = new S_Spawn();
                        foreach (Player p in _players.Values)
                        {
                            if (player != p)
                                spawnPacket.Objects.Add(p.Info);
                        }

                        // 현재 있는 map의 몬스터 동기화
                        foreach (NormalMonster m in _normalMonsters.Values)
                            spawnPacket.Objects.Add(m.Info);
                        foreach (BossMonster m in _bossMonsters.Values)
                            spawnPacket.Objects.Add(m.Info);


                        player.Session.Send(spawnPacket);
                    }
                }
                else if (type == GameObjectType.Normalmonster)
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
                    S_Spawn spawnPacket = new S_Spawn();
                    spawnPacket.Objects.Add(gameObject.Info);
                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != gameObject.Id)
                            p.Session.Send(spawnPacket);
                    }
                }
            }
        }

        // 관리 대상에서(딕셔너리) 삭제
        public void LeaveGame(int objectId)
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
            else if (type == GameObjectType.Normalmonster)
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
                S_Despawn despawnPacket = new S_Despawn();
                despawnPacket.ObjectIds.Add(objectId);
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
        public void HandleMove(Player player, C_Move movePacket)
        {
            if (player == null)
                return;

            // 서버에서 관리하기 위해 데이터 반영
            _players[player.Info.ObjectId].Info.PosX = movePacket.PosX;
            _players[player.Info.ObjectId].Info.PosY = movePacket.PosY;

            // 다른 플레이어한테도 알려준다
            S_Move resMovePacket = new S_Move();
            resMovePacket.ObjectId = player.Info.ObjectId;
            resMovePacket.PosX = movePacket.PosX;
            resMovePacket.PosY = movePacket.PosY;
            RecvPacketPlus();
            Broadcast(resMovePacket);
        }

        public void HandleSkill(Player player, C_Skill skillPacket)
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

        // 관리 대상에서(딕셔너리) 플레이어 찾기
        public GameObject FindPlayer(Func<GameObject, bool> condition)
        {
            foreach (Player player in _players.Values)
            {
                if (condition.Invoke(player))
                    return player;
            }

            return null;
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
