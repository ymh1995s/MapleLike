using Google.Protobuf.Protocol;
using ServerContents.Job;
using ServerContents.Object;
using System;


namespace ServerContents.Room
{
    // 아이템과 관련된 로직을 관리한다.
    // 경원님 작업하는 곳.
    public partial class GameRoom : JobSerializer
    {
        static Random random = new Random();

        public void ItemEnterGame(Player player, float posX, float posY)
        {
            lock (_lock)
            {
                if (player == null)
                    return;

                Item item = ObjectManager.Instance.Add<Item>();
                _items.Add(item.Id, item);
                item.Room = this;

                // 아이템이 게임룸에 입장한 사실을 모든 클라이언트에게 전송
                S_ItemSpawn spawnPacket = new S_ItemSpawn();
                spawnPacket.ItemType = GetRandomItem();
                Console.WriteLine(spawnPacket.ItemType);
                spawnPacket.CanOnlyOwnerLootTime = 15000; //15초
                spawnPacket.LifeTime = 30000; //30초
                spawnPacket.ItemInfo = item.Info;
                spawnPacket.ItemInfo.OwnerId = player.Id;
                spawnPacket.ItemInfo.PositionX = posX;
                spawnPacket.ItemInfo.PositionY = posY;
                
                Broadcast(spawnPacket);

                // 아이템 N초 후에 사라질 것을 예약함
                S_ItemDespawn deSpawnPacket = new S_ItemDespawn();
                deSpawnPacket.ItemId = item.Id;
                PushAfter(spawnPacket.CanOnlyOwnerLootTime, SetItemRootAnyOne, item.Id); // ex) 15초 후
                PushAfter(spawnPacket.LifeTime, LeaveItem, deSpawnPacket.ItemId); // ex) 30초 후
                
            }
        }

        // 아이템을 관리 대상에서(딕셔너리) 삭제
        // 경우 1. 최대 N초가 지나 맵에저 자연스럽게 사라질 때
        public void LeaveItem(int itemId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(itemId);

            Item item = null;

            // 이미 이 맵에 해당 아이템이 없다면 함수 종료
            if (_items.Remove(itemId, out item) == false)
                return;

            item.Room = null;

            // 아이템이 게임룸에서 퇴장한 사실을 모든 클라이언트에게 전송
            {
                S_ItemDespawn deSpawnPacket = new S_ItemDespawn();
                // TODO 아이템에 관한 정보 설정
                deSpawnPacket.ItemId = itemId;
                Broadcast(deSpawnPacket);
            }
        }

        public void SetItemRootAnyOne(int itemId)
        {
            Item item = null;

            // 이미 이 맵에 해당 아이템이 없다면 함수 종료
            if (_items.TryGetValue(itemId, out item) == false)
                return;

            item.Info.CanRootAnyOne = true;
        }

        public void ItemRooting(Player player, int itemId)
        {
            if(player == null) return;

            Item item = null;
            if (_items.TryGetValue(itemId, out item) == false)
                return;

            // 모두에게 소유권이 있거나, 소유권이 있는 사람이 아이템 루팅을 요청할 때
            if((item.Info.CanRootAnyOne) || (!item.Info.CanRootAnyOne && item.Info.OwnerId == player.Id))
            {
                if (_items.Remove(itemId, out item)==false)
                {
                    return;
                }
                
                item.Room = null;

                S_LootItem pkt = new S_LootItem();
                pkt.ItemId = itemId;
                pkt.PlayerId = player.Id;
                Broadcast(pkt);
            }
        }

        public static ItemType GetRandomItem()
        {
            double roll = random.NextDouble() * 100; // 0.0 ~ 99.9999...

            if (roll < 90.0) return ItemType.Gold;      // 90%
            if (roll < 94.5) return ItemType.Hppotion;  // 4.5%
            if (roll < 99.0) return ItemType.Mppotion;   // 4.5%
            // ItemType.Hppotion; 
            // 나머지 1%를 6개로 균등 배분 (각각 0.1667%)
            double remainingChance = roll - 99.0;
            int index = (int)(remainingChance / 0.1667); // 0 ~ 5 범위로 변환

            return (ItemType)(index + 1); // HELMET(1) ~ ARROW(6)
        }
    }
}
