using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using NUnit.Framework;
using UnityEngine;


// 기타 패킷 핸들러
public partial class PacketHandler
{
    public static event Action OnActivateStartScenePanel;

    public static int TestitemSpawnPktid; //테스트 코드
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        OnActivateStartScenePanel?.Invoke();
    }

    //드랍 
    public static void S_ItemSpawnHandler(PacketSession session, IMessage packet)
    {
        //실제 로직 기능 구현 
        S_ItemSpawn itemSpawnPkt = packet as S_ItemSpawn;
        TestitemSpawnPktid = itemSpawnPkt.PlayerId;
        
        // itemSpawnPkt.PlayerId // 몬스터를 죽인 플레이어 아이디  이걸로 연동하기 
        
        //먹는 시간은 아이템 정보를 클라이언트 단에서 시간초를 확인해라 
        
        // TODO 아이템 생기는 효과도 주기
        //  _objects 딕셔너리에 추가해주기 
        // 여기까지 했으면 모든 플레이어들에게 이 아이템이 떨어졌다고 보일거임!
        
        // 나오는건 확인 추가로 적이 죽었을때 위치를 받아서 나오게 하면됨 
        ObjectManager.Instance.SpwanItem(itemSpawnPkt);
        
    }

    //사라짐 
    public static void S_ItemDespawnHandler(PacketSession session, IMessage packet)
    {
        // 몇초 뒤 사라지는 기능  + 누가 먹으면  사라지게 보여야해 
        S_ItemDespawn itemDespawnPkt = packet as S_ItemDespawn;
        ObjectManager.Instance.DespwanItem(itemDespawnPkt);
        
        // 어떻게 아이템먹을때 호출 시킬까?
    }
    
    //먹기
    public static void S_LootItemHandler(PacketSession session, IMessage packet)
    {
        // 실제 로직 기능 구현
        S_LootItem lootItemPkt = packet as S_LootItem;
        if (lootItemPkt == null)
        {
            Debug.Log("아무것도 안들어옴");
            return;
        }

        var item = ObjectManager.Instance.FindById(lootItemPkt.ItemId);

        if (lootItemPkt.PlayerId == ObjectManager.Instance.MyPlayer.Id) // 내가 먹은 패킷인지 확인
        {
            if (item == null)
            {
                Debug.LogWarning("아이템을 찾을 수 없습니다.");
                return;
            }

            InitItem initItem = item.GetComponentInChildren<InitItem>();

            if (initItem != null && initItem.Ownerid == ObjectManager.Instance.MyPlayer.Id)
            {
                // 아이템이 나의 것일 경우
                var itemPickup = item.GetComponent<ItemPickup>();
                if (itemPickup != null && !itemPickup.isAttracting)
                {
                    itemPickup.StartAttracting(); // 아이템 흡수 시작
                    Debug.Log($"[내 아이템] lootItemPkt.PlayerId:{lootItemPkt.PlayerId} == MyPlayer.Id:{ObjectManager.Instance.MyPlayer.Id} → 아이템 흡수 시작");
                }
                else
                {
                    Debug.Log("아이템이 이미 흡수 중입니다.");
                }
            }
            else
            {
                Debug.Log($"[다른 사람 아이템] lootItemPkt.PlayerId:{lootItemPkt.PlayerId} != MyPlayer.Id:{ObjectManager.Instance.MyPlayer.Id} → 아이템 무시");
            }
        }
        
        // 아이템이 흡수 중인지 확인하고 흡수되지 않았다면 처리
        if (item != null && item.GetComponent<ItemPickup>().isAttracting == false)
        {
            Debug.Log("아이템 흡수 완료된 상태로 간주하고 아이템 제거");
            ObjectManager.Instance.DespwanItem2(lootItemPkt);
        }
    }
}
