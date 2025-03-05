using System;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Object = UnityEngine.Object;

public class ObjectManager : MonoBehaviour
{
    private static ObjectManager _instance;
    public static ObjectManager Instance { get { return _instance; } }
    public PlayerController MyPlayer { get; set; }
    // 동기화가 필요한 모든 오브젝트를 딕셔너리로 관리한다고 보면 됩니다.
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    // 원래 제가 했던 거에서는 Resources 폴더로 string으로 접근을 했었는데
    // 강사님이 비추하기도 했고 해서 Resources 폴더를 삭제하고 임시 프리팹 매칭 코드
    [SerializeField] GameObject tempPlayer;
    [SerializeField] GameObject tempMyPlayer;

    [SerializeField] GameObject worriorPrefab;
    [SerializeField] GameObject magicianPrefab;
    [SerializeField] GameObject archerPrefab;
    [SerializeField] GameObject ripPrefab;

    public GameObject myplayerTest;

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

    public void AddPlayer(PlayerInfo info, bool myPlayer = false )
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

                InitPlayer(go);

                MyPlayer = go.AddComponent<YHSMyPlayerController>();
                MyPlayer.GetComponent<YHSMyPlayerController>().playerInformation.InitPlayerInfo(info);
                MyPlayer.Id = info.PlayerId;
                _objects.Add(info.PlayerId, go);
                myplayerTest = go;
            }
            else
            {
                GameObject go = new GameObject();
                go.name = info.Name;
                go.layer = LayerMask.NameToLayer("Player");
                go.transform.position = new Vector3(info.PositionX, info.PositionY, 0f);

                InitPlayer(go);

                PlayerController pc = go.AddComponent<PlayerController>();
                pc.Id = info.PlayerId;

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
            go.transform.position = new Vector3(info.DestinationX, info.DestinationY, 0);

            _objects.Add(info.MonsterId, go);

            NormalMonsterController nmc = go.GetComponent<NormalMonsterController>();
            nmc.Id = info.MonsterId;
        }
        else if (objectType == GameObjectType.Bossmonster)
        {
            string monsterName = Regex.Replace(info.Name, @"_[0-9]+$", "");     // "_"와 뒤에 숫자 제거

            GameObject go = Instantiate(MonsterManager.Instance.GetMonsterPrefab(monsterName));
            go.name = info.Name;
            go.transform.position = new Vector3(info.DestinationX, info.DestinationY, 0);

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
        if (go.GetComponent<MonsterController>() != null)
        {
            if (go.TryGetComponent<NormalMonsterController>(out NormalMonsterController nmc))
                nmc.SetState(MonsterState.MDead);
            else if (go.TryGetComponent<BossMonsterController>(out BossMonsterController bmc))
                bmc.SetState(MonsterState.MDead);
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

    private void InitPlayer(GameObject go)
    {
        // 체력바나 이름표 등도 넣을 수 있지 않을까?

        GameObject character = Instantiate(tempMyPlayer, go.transform);     // 캐릭터 오브젝트
        character.name = "Character";

        GameObject rip = Instantiate(ripPrefab, go.transform);              // 비석 오브젝트
        rip.name = "RIP";
        rip.layer = LayerMask.NameToLayer("Props1");
        rip.SetActive(false);
    }
}