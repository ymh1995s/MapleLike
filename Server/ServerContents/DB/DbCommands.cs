using Google.Protobuf.Protocol;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.Json;

namespace ServerContents.DB
{
    public class ItemJsonData
    {
        public int id { get; set; }
        public string itemName { get; set; }
        public string description { get; set; }
        public int? sellprice { get; set; }
        public int? buyprice { get; set; }
        public string itemType { get; set; }
        public string itemCategory { get; set; }

        public int? healAmount { get; set; }
        public int? MpAmount { get; set; }

        // Equipment 전용
        public int? limitLevel { get; set; }
        public string classType { get; set; }
        public string parts { get; set; }
        public int? attackPower { get; set; }
        public int? magicPower { get; set; }
        public int? defensePower { get; set; }
    }

    class DbCommands
    {
        static string itemJsonPath = "../../../Items.Json";

        // 초기화 시간이 좀 걸림
        public static void InitializeDB(bool forceReset = false)
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
    }
}
