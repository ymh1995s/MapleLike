using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore.Storage.Json;
using ServerContents.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerContents.Room
{
    #region 몬스터 데이터 관련
    public class MonsterData
    {
        public string name { get; set; }
        public string type { get; set; }
        public int level { get; set; }
        public int hp { get; set; }
        public int attackPower { get; set; }
        public int defense { get; set; }
        public float speed { get; set; }
        public int exp { get; set; }
    }

    public class MonsterDatabase
    {
        public List<MonsterData> monsterDatabase { get; set; }
    }
    #endregion

    #region 맵 데이터 관련
    public class RangeData
    {
        public float minX { get; set; }
        public float maxX { get; set; }
        public float Y { get; set; }
    }

    public class LevelRangeData
    {
        public int minLevel { get; set; }
        public int maxLevel { get; set; }
    }

    public class ZoneData
    {
        public int id { get; set; }
        public string monsterType { get; set; }                    // 해당 존에 스폰 가능한 몬스터의 타입
        public RangeData range { get; set; }
        public LevelRangeData levelRange { get; set; }
        public int zoneMaxMonsterCount { get; set; }
    }

    public class MapData
    {
        public int id { get; set; }
        public string mapType { get; set; }
        public int mapMaxMonsterCount { get; set; }
        public int zoneCount { get; set; }
        public List<ZoneData> zones { get; set; }
    }

    public class MapDatabase
    {
        public List<MapData> mapDatabase { get; set; }
    }

    #endregion
    public class Zone
    {
        public Zone(
            int id,
            string monsterType,
            int maxMonsterCount,
            Tuple<Tuple<float, float>, float> range,
            Tuple<int, int> levelRange)
        {
            this.id = id;
            this.monsterType = monsterType;
            this.maxMonsterCount = maxMonsterCount;
            this.currentMonsterCount = 0;
            this.range = range;
            this.levelRange = levelRange;
        }

        public int id;
        public string monsterType;
        public int maxMonsterCount;
        public int currentMonsterCount;
        public Tuple<Tuple<float, float>, float> range;
        public Tuple<int, int> levelRange;
    }

    public class Map
    {
        public Map(
            string mapType,
            int maxMonsterCount,
            int zoneCount
            )
        {
            this.mapType = mapType;
            this.maxMonsterCount = maxMonsterCount;
            this.currentMonsterCount = 0;
            this.zoneCount = zoneCount;
        }

        public string mapType;
        public int maxMonsterCount;
        public int currentMonsterCount;
        public int zoneCount;
        public Dictionary<int, Zone> zones = new Dictionary<int, Zone>();
    }

    public class MonsterManager
    {
        Random rnd = new Random();

        public static MonsterManager Instance { get; } = new MonsterManager();
        private bool isAlreadyInitialize = false;

        private MonsterDatabase? monsterDatabase;
        private MapDatabase? mapDatabase;

        private Dictionary<int, Map> maps = new Dictionary<int, Map>();

        // (MonsterId, (roomId, zoneId))
        private Dictionary<int, Tuple<int, int>> allMonsters = new Dictionary<int, Tuple<int, int>>();

        public void MonsterSpawn(int currentRoomId)
        {
            if (!maps.TryGetValue(currentRoomId, out Map currentMap))
                return;
            
            if (monsterDatabase == null || monsterDatabase.monsterDatabase == null)
                return;

            // TODO: 보스맵의 경우, 해당 맵에 사람이 없을 때만 스폰이 되도록 처리해야함.
            if (currentMap.mapType == "Boss")
            {

            }

            foreach (var zone in currentMap.zones)
            {
                while (zone.Value.currentMonsterCount < zone.Value.maxMonsterCount)
                {
                    float spawnPosX = (float)(rnd.NextDouble() * (zone.Value.range.Item1.Item2 - zone.Value.range.Item1.Item1) + zone.Value.range.Item1.Item1);
                    float spawnPosY = zone.Value.range.Item2;

                    int minLevel = zone.Value.levelRange.Item1;
                    int maxLevel = zone.Value.levelRange.Item2;
                    string spawnableMonsterType = zone.Value.monsterType;
                    
                    // Type, LevelRange에 해당하는 몬스터 필터링
                    var validMonsters = monsterDatabase.monsterDatabase
                        .Where(monster => monster.type == spawnableMonsterType && monster.level >= minLevel && monster.level <= maxLevel)
                        .ToList();

                    if (validMonsters.Count > 0)
                    {
                        // 랜덤으로 하나 선택
                        MonsterData selectedMonster = validMonsters[rnd.Next(validMonsters.Count)];

                        Console.WriteLine($"{currentMap.mapType} Map {currentRoomId}의 Zone {zone.Value.id}에서 {selectedMonster.type} {selectedMonster.name} (Lv.{selectedMonster.level})가 스폰됨! 위치: ({spawnPosX}, {spawnPosY})");

                        Monster monster = null;
                        if (spawnableMonsterType == "Normal")
                            monster = ObjectManager.Instance.Add<NormalMonster>();
                       else if(spawnableMonsterType == "Boss")
                            monster = ObjectManager.Instance.Add<BossMonster>();

                        monster.Info.Name = $"{selectedMonster.name}_{monster.Info.MonsterId}";
                        monster.Info.DestinationX = spawnPosX;
                        monster.Info.DestinationY = spawnPosY;
                        monster.Info.StatInfo = GetMonsterStatInfo(selectedMonster);
                        monster.Stat = monster.Info.StatInfo;
                        monster.Info.CreatureState = MonsterState.MIdle;

                        monster.SetInitialPos(new Vector2(spawnPosX, spawnPosY));
                        monster.SetBoundPos(new Vector2(zone.Value.range.Item1.Item1, zone.Value.range.Item1.Item2));

                        RoomManager.Instance.Find(currentRoomId).MonsterEnterGame(monster);

                        zone.Value.currentMonsterCount++;
                        currentMap.currentMonsterCount++;

                        Tuple<int, int> monsterSpawnLocation = new Tuple<int, int>(currentRoomId, zone.Value.id);
                        allMonsters.Add(monster.Info.MonsterId, monsterSpawnLocation);
                    }
                }
            }
        }

        private MonsterStatInfo GetMonsterStatInfo(MonsterData monsterData)
        {
            MonsterStatInfo monsterStatInfo = new MonsterStatInfo();

            monsterStatInfo.Level = monsterData.level;
            monsterStatInfo.Hp = monsterData.hp;
            monsterStatInfo.AttackPower = monsterData.attackPower;
            monsterStatInfo.Defense = monsterData.defense;
            monsterStatInfo.Speed = monsterData.speed * 0.05f;
            monsterStatInfo.Exp = monsterData.exp;

            return monsterStatInfo;
        }


        public void MonsterDespawn(int monsterId)
        {
            // monsterId가 allMonsters에 존재하는지 확인
            if (!allMonsters.ContainsKey(monsterId))
            {
                return; // 키를 찾지 못했을 경우 메서드 종료
            }

            int roomId = allMonsters[monsterId].Item1;
            int zoneId = allMonsters[monsterId].Item2;

            maps[roomId].zones[zoneId].currentMonsterCount--;
            maps[roomId].currentMonsterCount--;

            allMonsters.Remove(monsterId);
        }

        private void InitializeMap(MapDatabase mapDatabase)
        {
            foreach (MapData mapData in mapDatabase.mapDatabase)
            {
                Map newMap = new Map(mapData.mapType, mapData.mapMaxMonsterCount, mapData.zoneCount);

                foreach (ZoneData zoneData in mapData.zones)
                {       
                    // Zone의 range와 levelRange 데이터를 Tuple로 변환
                    var range = new Tuple<Tuple<float, float>, float>(
                        new Tuple<float, float>(zoneData.range.minX, zoneData.range.maxX),
                        zoneData.range.Y
                    );

                    var levelRange = new Tuple<int, int>(zoneData.levelRange.minLevel, zoneData.levelRange.maxLevel);

                    // 새로운 Zone 객체 생성 후 newMap의 zones 리스트에 추가
                    Zone newZone = new Zone(zoneData.id, zoneData.monsterType, zoneData.zoneMaxMonsterCount, range, levelRange);
                    newMap.zones.Add(zoneData.id, newZone);
                }
                maps.Add(mapData.id, newMap);
            }
        }

        // GameRoom의 Init 함수에서 한 번의 호출 맵 데이터, 몬스터 데이터를 읽어옮.
        public void LoadAllData()
        {
            if (!isAlreadyInitialize)
            {
                LoadMonsterData();
                LoadMapData();
                isAlreadyInitialize = true;
            }
        }

        private void LoadMonsterData()
        {
            // 프로젝트의 루트 폴더를 기준으로 경로 설정.
            // TODO: Json 파일을 불러들일 더 정확한 방법을 고안해 수정 필요.
            string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string path = Path.Combine(projectRoot, "Room", "Data", "Monsters.Json");

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                monsterDatabase = JsonSerializer.Deserialize<MonsterDatabase>(json);
            }
            else
            {
                Console.WriteLine("Monsters.Json 파일이 존재하지 않습니다.");
            }
        }

        private void LoadMapData()
        {
            // 프로젝트의 루트 폴더를 기준으로 경로 설정.
            // TODO: Json 파일을 불러들일 더 정확한 방법을 고안해 수정 필요.
            string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
            string path = Path.Combine(projectRoot, "Room", "Data", "Maps.Json");

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                mapDatabase = JsonSerializer.Deserialize<MapDatabase>(json);
                InitializeMap(mapDatabase);
            }
            else
            {
                Console.WriteLine("Maps.Json 파일이 존재하지 않습니다.");
            }
        }
    }
}
