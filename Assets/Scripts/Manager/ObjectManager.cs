using System;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

public class ObjectManager : MonoBehaviour
{
    private static ObjectManager _instance;
    public static ObjectManager Instance { get { return _instance; } }
    public PlayerController MyPlayer { get; set; }
    // 동기화가 필요한 모든 오브젝트를 딕셔너리로 관리한다고 보면 됩니다.
    public Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    // 원래 제가 했던 거에서는 Resources 폴더로 string으로 접근을 했었는데
    // 강사님이 비추하기도 했고 해서 Resources 폴더를 삭제하고 임시 프리팹 매칭 코드
    [SerializeField] GameObject tempPlayer;
    [SerializeField] GameObject tempMyPlayer;

    [SerializeField] List<GameObject> classPrefabList;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }

    
    public void SpwanItem(S_ItemSpawn itemSpawn)
    {
        Addressables.LoadResourceLocationsAsync("ItemObj").Completed += (handle) =>
        {
            var locations = handle.Result;
            if (locations == null || locations.Count == 0)
            {
                Debug.LogError("ItemObj에 대한 리소스를 찾을 수 없습니다.");
                return;
            }

            locations = locations.OrderBy(loc => loc.PrimaryKey).ToList();

            // ItemType에 맞는 location 찾기
            IResourceLocation selectedLocation = null;
            foreach (var location in locations)
            {
                string itemTypeString = itemSpawn.ItemType.ToString();
                if (location.PrimaryKey.Contains(itemSpawn.ItemType.ToString())) // 아이템 이름 포함 여부 확인
                {
                    selectedLocation = location;
                    // 매칭된 경우 디버그 로그 출력
                    Debug.Log($"✅ 매칭 성공: {location.PrimaryKey} == {itemTypeString}");
                    break;
                }
            }

            if (selectedLocation == null)
            {
                return;
            }
            // 아이템 생성
            var obj = Addressables.InstantiateAsync(selectedLocation, 
                new Vector3(itemSpawn.ItemInfo.PositionX, itemSpawn.ItemInfo.PositionY, 0), 
                Quaternion.identity);

            obj.Completed += (instanceHandle) =>
            {
                if (instanceHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    var spawnedObj = instanceHandle.Result;
                    _objects.Add(itemSpawn.ItemInfo.ItemId, spawnedObj);
                    spawnedObj.transform.GetComponentInChildren<InitItem>().Serverid = itemSpawn.ItemInfo.ItemId;

                    Debug.Log($"x: {itemSpawn.ItemInfo.PositionX}, y: {itemSpawn.ItemInfo.PositionY}");
                    Debug.Log($"Spawned Object Name: {spawnedObj.gameObject.name}");
                }
                else
                {
                    Debug.LogError("아이템 생성 실패!");
                }
            };
        };
    
    }

    
    public void PickupNearbyItems2()
    {
        
        var player = FindById(MyPlayer.Id);
        // 플레이어의 아이템 줍기 범위 및 아이템 레이어 정보 가져오기
        var pickupRange = player.GetComponent<InputManager>().pickupRange;
        var itemLayer = player.GetComponent<InputManager>().itemLayer;
        //플레이어 가져오기
       
        // 특정 범위 내에 있는 아이템 찾기
        Collider2D[] items = Physics2D.OverlapCircleAll(player.transform.position, pickupRange, itemLayer);
        Debug.Log("Found " + items.Length + " nearby items");

        C_LootItem lootItem = new C_LootItem();
        List<InitItem> nearbyItems = new List<InitItem>();

        foreach (Collider2D item in items)
        {
            InitItem itemInfo = item.GetComponentInChildren<InitItem>();
            if (itemInfo != null)
            {
                nearbyItems.Add(itemInfo);
            }
        }

        // 아이템 정보가 있을 경우에만 서버에 전송
        foreach (var VARIABLE in Instance._objects.Keys)
        {
            foreach (var itemInfo in nearbyItems)
            {
                if (VARIABLE == itemInfo.Serverid)
                {
                    lootItem.ItemId = itemInfo.Serverid;
                    NetworkManager.Instance.Send(lootItem);
                    Debug.Log("itemPickup:" + lootItem.ItemId);
                    break;
                }
            }
        }
    }
    

    public void DespwanItem(S_ItemDespawn itemDespawnPkt)
    {
        Addressables.ReleaseInstance(FindById(itemDespawnPkt.ItemId));
        _objects.Remove(itemDespawnPkt.ItemId);
        Debug.Log(FindById(itemDespawnPkt.ItemId)); 
    }
    
    public void DespwanItem2(S_LootItem itemDespawnPkt)
    {
        Addressables.ReleaseInstance(FindById(itemDespawnPkt.ItemId));
        _objects.Remove(itemDespawnPkt.ItemId);
        Debug.Log(FindById(itemDespawnPkt.ItemId)); 
    }

    public void AddPlayer(PlayerInfo info, bool myPlayer = false)
    {
        GameObjectType objectType = GetObjectTypeById(info.PlayerId);
        if (objectType == GameObjectType.Player)
        {
            if (myPlayer)
            {
                // TODO: 코드 정리 필요
                GameObject go = new GameObject();   // 오브젝트 모음
                go.name = info.Name;
                go.layer = LayerMask.NameToLayer("MyPlayer");
                go.transform.position = new Vector3(info.PositionX, info.PositionY, 0f);

                InitPlayer(go, (int)info.StatInfo.ClassType);

                MyPlayer = go.AddComponent<YHSMyPlayerController>();
                MyPlayer.GetComponent<YHSMyPlayerController>().playerInformation.InitPlayerInfo(info);
                MyPlayer.Id = info.PlayerId;

                MyPlayer.SetDestination(info.PositionX, info.PositionY);
                MyPlayer.SetPlayerState(info.CreatureState);
                
                Debug.Log("이사람의 직업:"+PlayerInformation.playerStatInfo.ClassType);

                // #region weapon아이템 찾기
                // // --- WeaponItem 찾기 ---
                // // Transform weaponItem = FindDeepChildLinq(go.transform, "WeaponItem");
                // var weaponItem = UIManager.Instance.EquipSlots.Find(slot => slot.name == "WeaponItem");
                // if (weaponItem != null)
                // {
                //     Debug.Log("WeaponItem 발견: " + weaponItem.name);
                //     
                //     // foreach (var VARIABLE in ItemManager.Instance.ItemList)
                //     // {
                //     //     //(경원)임시 현승님 오시면 수정 사항 
                //     //     //수정을 어떻게 해야되나 직업 클래스 타입으로 받아서 넣어야 한다.
                //     //     //현재는  WeaponItem의 무기 타입을 보고 넣고있다.
                //     //     //이렇게 넣으면 무기가 많아지면 무기타입만 보고 넣기에는  오류가 날 것으로 예상 
                //     //     if (VARIABLE.ItemType == temp.CurrentItemType)
                //     //     {
                //     //         temp.CurrentItem = VARIABLE;
                //     //         temp._image.sprite = VARIABLE.IconSprite;
                //     //         Color color = temp._image.color;
                //     //         color.a = 1f;  // 
                //     //         temp._image.color = color;
                //     //         
                //     //         var equipmentstat = PlayerInformation.equipmentStat;
                //     //         if (temp.CurrentItem is Equipment eq)
                //     //         {
                //     //             equipmentstat.AttackPower = eq.attackPower;
                //     //             equipmentstat.Defense = eq.defensePower;
                //     //             equipmentstat.MagicPower = eq.magicPower;
                //     //             Debug.Log("초기값 갱신");
                //     //             Debug.Log(equipmentstat.AttackPower);
                //     //         }
                //     //         
                //     //         break;
                //     //     }
                //     // }
                //
                // }
                // else
                // {
                //     Debug.Log("WeaponItem 못찾음 ");
                // }
                // #endregion


                _objects.Add(info.PlayerId, go);
            }
            else
            {
                GameObject go = new GameObject();
                go.name = info.Name;
                go.layer = LayerMask.NameToLayer("Player");

                InitPlayer(go, (int)info.StatInfo.ClassType);

                PlayerController pc = go.AddComponent<PlayerController>();
                pc.Id = info.PlayerId;

                // 다른 플레이어의 위치, 상태 동기화를 위해 필요한 부분
                pc.transform.position = new Vector3(info.PositionX, info.PositionY, 0f);
                pc.SetDestination(info.PositionX, info.PositionY);
                pc.SetPlayerState(info.CreatureState);

                _objects.Add(info.PlayerId, go);
            }
        }
    }

    public void AddMonster(MonsterInfo info)
    {
        GameObjectType objectType = GetObjectTypeById(info.MonsterId);

        if (objectType == GameObjectType.Normalmonster)
        {
            string monsterName = Regex.Replace(info.Name, @"_[0-9]+$", "");     // "_"와 뒤에 숫자 제거

            GameObject go = Instantiate(MonsterManager.Instance.GetMonsterPrefab(monsterName));
            go.name = info.Name;
            go.transform.position = new Vector3(info.DestinationX, info.DestinationY, 0f);

            go.GetComponent<MonsterController>().UpdateInfo(info);
            go.GetComponent<BaseController>().SetDestination(info.DestinationX, info.DestinationY);

            _objects.Add(info.MonsterId, go);

            NormalMonsterController nmc = go.GetComponent<NormalMonsterController>();
            nmc.Id = info.MonsterId;
        }
        else if (objectType == GameObjectType.Bossmonster)
        {
            string monsterName = Regex.Replace(info.Name, @"_[0-9]+$", "");     // "_"와 뒤에 숫자 제거

            GameObject go = Instantiate(MonsterManager.Instance.GetMonsterPrefab(monsterName));
            go.name = info.Name;
            go.transform.position = new Vector3(info.DestinationX, info.DestinationY, 0f);

            go.GetComponent<MonsterController>().UpdateInfo(info);
            go.GetComponent<BaseController>().SetDestination(info.DestinationX, info.DestinationY);

            _objects.Add(info.MonsterId, go);

            BossMonsterController bmc = go.GetComponent<BossMonsterController>();
            bmc.Id = info.MonsterId;
        }
    }

    public void Remove(int id)
    {
        GameObject go = FindById(id);
        if (go == null)
            return;

        // 몬스터의 경우, 각 스크립트에서 Destroy 처리.
        if (go.TryGetComponent<MonsterController>(out MonsterController mc))
        {
            if (go.TryGetComponent<NormalMonsterController>(out NormalMonsterController nmc))
                nmc.SetState(MonsterState.MDead);
            else if (go.TryGetComponent<BossMonsterController>(out BossMonsterController bmc))
                bmc.SetState(MonsterState.MDead);

            mc.SetCurrentHp(0);
        }
        else
            Object.Destroy(go); // Unity 메인 스레드에서 오브젝트 삭제하고
        _objects.Remove(id); // 딕셔너리에서 제거한다.
    }

    // 딕셔너리에서 대상 오브젝트를 가져온다.
    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public void Clear()
    {
        foreach (GameObject obj in _objects.Values)
            Object.Destroy(obj); // Unity 메인 스레드에서 오브젝트 삭제하고

        _objects.Clear(); // 딕셔너리 클리어
        MyPlayer = null;
    }

    private void InitPlayer(GameObject go, int classIndex)
    {
        // 체력바나 이름표 등도 넣을 수 있지 않을까?

        {
            // 직업에 따른 프리팹 인스턴싱
            GameObject character = Instantiate(classPrefabList[classIndex], go.transform);     // 캐릭터 오브젝트
            character.name = "Character";
        }
    }
}