using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngine.AddressableAssets;


// 기타 패킷 핸들러
public partial class PacketHandler
{
    public static event Action OnActivateStartScenePanel;
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        OnActivateStartScenePanel?.Invoke();
    }

    //드랍 
    public static void S_ItemSpawnHandler(PacketSession session, IMessage packet)
    {
        //실제 로직 기능 구현 
        S_ItemSpawn itemSpawnPkt = packet as S_ItemSpawn;
  
        // TODO 아이템 생기는 효과도 주기
        //  _objects 딕셔너리에 추가해주기 
        // 여기까지 했으면 모든 플레이어들에게 이 아이템이 떨어졌다고 보일거임!
        
        // 나오는건 확인 추가로 적이 죽었을때 위치를 받아서 나오게 하면됨 
        ObjectManager.Instance.SpwanItem(itemSpawnPkt);
        
    }

    //사라짐 
    public static void S_ItemDespawnHandler(PacketSession session, IMessage packet)
    {
        // 몇초 뒤 사라지는 기능 
        S_ItemDespawn itemDespawnPkt = packet as S_ItemDespawn;
        ObjectManager.Instance.DespwanItem(itemDespawnPkt);
        
    }

    
    //먹기
    public static void S_LootItemHandler(PacketSession session, IMessage packet)
    {
        //실제 로직 기능 구현 
        S_LootItem lootItemPkt = packet as S_LootItem;

        
        if (lootItemPkt.PlayerId == ObjectManager.Instance.MyPlayer.Id )
        {
            Debug.Log($"Player {ObjectManager.Instance.MyPlayer.Id} has loot item");
            Debug.Log($"ItemID:{lootItemPkt.ItemId}");
            var item = ObjectManager.Instance.FindById(lootItemPkt.ItemId);
            //누가 죽였는지 알아야함 
            if (item.GetComponentInChildren<InitItem>().Ownerid == ObjectManager.Instance.MyPlayer.Id)
            {
                Debug.Log($"Player {ObjectManager.Instance.MyPlayer.Id} has looted the item.");
                Debug.Log($"ItemID: {lootItemPkt.ItemId}");
                item.GetComponent<ItemPickup>().StartAttracting(ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id).transform);
            }else
            {
                Debug.Log("이 아이템은 내가 죽인 몬스터에서 나온 것이 아님!");
            }
        
        }
        else
        {
            Debug.Log("다른이가 먹었음 "+ObjectManager.Instance.MyPlayer.Id);
            Debug.Log($"ItemID:{lootItemPkt.ItemId}");
        }
        
        
    }
}
