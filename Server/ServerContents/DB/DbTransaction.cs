using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using ServerContents.Job;
using ServerContents.Object;
using ServerContents.Room;
using System.Text.Json;

namespace ServerContents.DB
{
    class DbTransaction : JobSerializer
    {
        public static DbTransaction Instance { get; } = new DbTransaction();

        public static void SavePlayerInfoToDbReq(Player player)
        {
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    // 1. DbId가 있는 유저 테이블을 찾고
                    // 2. 인벤토리까지 함께 로드함
                    UserDb findAccount = db.Users
                        .Include(u => u.Inventory)
                        .FirstOrDefault(a => a.DbId == player.Info.DbId);

                    if (findAccount == null)
                    {
                        var newUser = new UserDb
                        {
                            DbId = player.Info.DbId,
                            Level = player.Info.StatInfo.Level,
                            ClassType = player.Info.StatInfo.ClassType,
                            MapNo = player.Info.MapNo,
                            CurrentExp = player.Info.StatInfo.CurrentExp,
                            MaxExp = player.Info.StatInfo.MaxExp,
                            CurrentHp = player.Info.StatInfo.CurrentHp,
                            MaxHp = player.Info.StatInfo.MaxHp,
                            CurrentMp = player.Info.StatInfo.CurrentMp,
                            MaxMp = player.Info.StatInfo.MaxMp,
                            AttackPower = player.Info.StatInfo.AttackPower,
                            MagicPower = player.Info.StatInfo.MagicPower,
                            Defense = player.Info.StatInfo.Defense,
                            Speed = player.Info.StatInfo.Speed,
                            Jump = player.Info.StatInfo.Jump,
                            STR = player.Info.StatInfo.STR,
                            DEX = player.Info.StatInfo.DEX,
                            INT = player.Info.StatInfo.INT,
                            LUK = player.Info.StatInfo.LUK,
                            Inventory = new List<InventoryDb>()
                        };

                        foreach (ItemInfo item in player.Info.Inventory.ItemInfo)
                        {
                            newUser.Inventory.Add(new InventoryDb
                            {
                                ItemDbId = item.ItemType,
                                Count = item.ItemCount,
                                MaxCount = 9999, // 필요하면 설정
                                IsEquipped = 0, // 기본값
                                UserDbId = player.Info.DbId
                            });
                        }

                        db.Users.Add(newUser);
                    }
                    else
                    {
                        // 기존 유저 정보 수정
                        findAccount.DbId = player.Info.DbId;
                        findAccount.Level = player.Info.StatInfo.Level;
                        findAccount.ClassType = player.Info.StatInfo.ClassType;
                        findAccount.MapNo = player.Info.MapNo;
                        findAccount.CurrentExp = player.Info.StatInfo.CurrentExp;
                        findAccount.MaxExp = player.Info.StatInfo.MaxExp;
                        findAccount.CurrentHp = player.Info.StatInfo.CurrentHp;
                        findAccount.MaxHp = player.Info.StatInfo.MaxHp;
                        findAccount.CurrentMp = player.Info.StatInfo.CurrentMp;
                        findAccount.MaxMp = player.Info.StatInfo.MaxMp;
                        findAccount.AttackPower = player.Info.StatInfo.AttackPower;
                        findAccount.MagicPower = player.Info.StatInfo.MagicPower;
                        findAccount.Defense = player.Info.StatInfo.Defense;
                        findAccount.Speed = player.Info.StatInfo.Speed;
                        findAccount.Jump = player.Info.StatInfo.Jump;
                        findAccount.STR = player.Info.StatInfo.STR;
                        findAccount.DEX = player.Info.StatInfo.DEX;
                        findAccount.INT = player.Info.StatInfo.INT;
                        findAccount.LUK = player.Info.StatInfo.LUK;

                        foreach (var item in player.Info.Inventory.ItemInfo)
                        {
                            SaveItemInfoToDb(db, player, findAccount, item, isNew: false);
                        }
                    }
                    db.SaveChanges();
                }
            });
        }

        public static void SaveItemInfoToDbReq(Player player, ItemInfo itemInfo)
        {
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    // 1. DbId가 있는 유저 테이블을 찾고
                    // 2. 인벤토리까지 함께 로드함
                    UserDb findAccount = db.Users
                        .Include(u => u.Inventory)
                        .FirstOrDefault(a => a.DbId == player.Info.DbId);

                    SaveItemInfoToDb(db, player, findAccount, itemInfo, isNew: true);

                    db.SaveChanges();
                }
            });
        }

        public static void SaveItemInfoToDb(AppDbContext db, Player player,  UserDb user, ItemInfo itemInfo, bool isNew = true)
        {
            if (user == null)
                return;

            // 동일한 아이템이 이미 있는지 검사
            InventoryDb existingItem = user.Inventory
            .FirstOrDefault(i =>
                i.UserDbId == player.Info.DbId &&
                i.ItemDbId == itemInfo.ItemType);

            if (existingItem != null)
            {
                if (itemInfo.Itemstate == ItemState.IsEquipped)
                {
                    // 장비면 장착상태 변경
                    existingItem.IsEquipped = itemInfo.Itemstate;
                }
                else if (itemInfo.IsFromEquipped == true)
                {
                    existingItem.IsEquipped = itemInfo.Itemstate;
                }
                else if (itemInfo.ItemCount <= 0)
                {
                    // 개수가 0 이하이면 해당 인벤토리 항목 삭제
                    db.Inventories.Remove(existingItem);
                }
                else if ((int)existingItem.ItemDbId >= 1000 && isNew == true)
                {
                    // TODO : 1000  넘으면 무조건 장비니까 그냥 새로운 장비 넣어야함 => 1000 하드코딩 제거
                    // 없으면 새 인벤토리 행 생성
                    InventoryDb newInventoryItem = new InventoryDb
                    {
                        UserDbId = user.DbId,
                        ItemDbId = itemInfo.ItemType,
                        Count = itemInfo.ItemCount,
                        MaxCount = 1, // 필요에 따라 설정
                        IsEquipped = 0
                    };
                    db.Inventories.Add(newInventoryItem);
                }
                else
                {
                    // 수량 갱신
                    existingItem.Count = itemInfo.ItemCount;
                }
            }
            else
            {
                // 없으면 새 인벤토리 행 생성
                InventoryDb newInventoryItem = new InventoryDb
                {
                    UserDbId = user.DbId,
                    ItemDbId = itemInfo.ItemType,
                    Count = itemInfo.ItemCount,
                    MaxCount = 99, // 필요에 따라 설정
                    IsEquipped = 0
                };
                db.Inventories.Add(newInventoryItem);
            }
        }


        #region 아이템 초기화 (DB 초기화 시 1회)
        static string itemJsonPath = "../../../Items.Json";

        // 초기화 시간이 좀 걸림
        // 서버가 첫 실행될 때 1회 실행되는 함수
        public static void InitializeDB(bool forceReset = false)
        {
            Instance.Push(() =>
            {
                using (AppDbContext db = new AppDbContext())
                {
                    var databaseCreator = db.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;

                    if (forceReset)
                    {
                        // 강제로 삭제 후 생성
                        db.Database.EnsureDeleted();
                        db.Database.EnsureCreated();
                        CreateItemData(db, itemJsonPath);
                        Console.WriteLine("DB forcibly reset and initialized.");
                    }
                    else
                    {
                        // DB가 존재하지 않으면 생성 및 데이터 삽입
                        if (!databaseCreator.Exists())
                        {
                            db.Database.EnsureCreated();
                            CreateItemData(db, itemJsonPath);
                            Console.WriteLine("DB created and initialized because it did not exist.");
                        }
                        else
                        {
                            // DB 존재 시 아이템 데이터가 없으면 삽입
                            if (!db.Items.Any())
                            {
                                CreateItemData(db, itemJsonPath);
                                Console.WriteLine("DB exists but no item data found; data inserted.");
                            }
                            else
                            {
                                Console.WriteLine("DB and item data already exist; no action taken.");
                            }
                        }
                    }
                }
            });
        }

        public static void CreateItemData(AppDbContext db, string jsonPath)
        {
            string json = File.ReadAllText(jsonPath);

            var root = JsonSerializer.Deserialize<JsonElement>(json);
            var items = root.GetProperty("items");

            foreach (var item in items.EnumerateArray())
            {
                string category = item.GetProperty("itemCategory").GetString();
                ItemType id = (ItemType)item.GetProperty("id").GetInt32();
                string name = item.GetProperty("itemName").GetString();
                string desc = item.GetProperty("description").GetString();
                int sell = item.TryGetProperty("sellprice", out var sp) && sp.TryGetInt32(out int s) ? s : 0;
                int buy = item.TryGetProperty("buyprice", out var bp) && bp.TryGetInt32(out int b) ? b : 0;

                var itemDb = new ItemDb
                {
                    ItemDbId = id, // 명시적 PK 삽입
                    Name = name,
                    Type = Enum.TryParse<ItemType>(item.GetProperty("itemType").GetString(), out var typeVal) ? typeVal : ItemType.Inone,
                    Description = desc,
                    SellPrice = sell,
                    BuyPrice = buy,
                };

                db.Items.Add(itemDb);

                if (category == "Consumable")
                {
                    var consumable = new ItemConsumableDb
                    {
                        ItemDbId = id,
                        HealHp = item.TryGetProperty("healAmount", out var hp) && hp.TryGetInt32(out int hval) ? hval : 0,
                        HealMp = item.TryGetProperty("MpAmount", out var mp) && mp.TryGetInt32(out int mpval) ? mpval : 0
                    };
                    db.ItemConsumables.Add(consumable);
                }
                else if (category == "Equipment")
                {
                    var equip = new ItemEquipDb
                    {
                        ItemDbId = id,
                        RequiredJob = item.TryGetProperty("classType", out var job) ? job.GetString() : null,
                        AttackPower = item.TryGetProperty("attackPower", out var atk) && atk.TryGetInt32(out int atkVal) ? atkVal : 0,
                        MagicPower = item.TryGetProperty("magicPower", out var mag) && mag.TryGetInt32(out int magVal) ? magVal : 0,
                        Defense = item.TryGetProperty("defensePower", out var def) && def.TryGetInt32(out int defVal) ? defVal : 0,
                        Part = item.TryGetProperty("parts", out var part) ? part.GetString() : "Unknown"  // 기본값 넣기
                    };
                    db.ItemEquips.Add(equip);
                }
            }

            db.SaveChanges();
        }
        #endregion 아이템 초기화 (DB 초기화 시 1회)
    }
}
