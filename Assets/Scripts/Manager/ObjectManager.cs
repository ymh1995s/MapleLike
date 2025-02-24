using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour
{
    private static ObjectManager _instance;
    public static ObjectManager Instance { get { return _instance; } }
    public MyPlayerController MyPlayer { get; set; }
    // 동기화가 필요한 모든 오브젝트를 딕셔너리로 관리한다고 보면 됩니다.
    Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    // 원래 제가 했던 거에서는 Resources 폴더로 string으로 접근을 했었는데
    // 강사님이 비추하기도 했고 해서 Resources 폴더를 삭제하고 임시 프리팹 매칭 코드
    [SerializeField] GameObject tempPlayer;
    [SerializeField] GameObject tempMyPlayer;
    [SerializeField] GameObject tempNormalMonster;
    [SerializeField] GameObject tempBossMonster;


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

    public void AddPlayer(PlayerInfo info, bool myPlayer = false)
    {
        GameObjectType objectType = GetObjectTypeById(info.PlayerId);

        if (objectType == GameObjectType.Player)
        {
            if (myPlayer)
            {
                GameObject go = Instantiate(tempMyPlayer);
                go.name = info.Name;
                _objects.Add(info.PlayerId, go);

                MyPlayer = go.GetComponent<MyPlayerController>();
                MyPlayer.Id = info.PlayerId;
            }
            else
            {
                GameObject go = Instantiate(tempPlayer);
                go.name = info.Name;
                _objects.Add(info.PlayerId, go);

                PlayerController pc = go.GetComponent<PlayerController>();
                pc.Id = info.PlayerId;
            }
        }
    }

    public void AddMonster(MonsterInfo info)
    {
        GameObjectType objectType = GetObjectTypeById(info.MonsterId);

        if (objectType == GameObjectType.Normalmonster)
        {
            GameObject go = Instantiate(tempNormalMonster);
            go.name = info.Name;
            _objects.Add(info.MonsterId, go);

            NormalMonsterController nmc = go.GetComponent<NormalMonsterController>();
            nmc.Id = info.MonsterId;
        }
        else if (objectType == GameObjectType.Bossmonster)
        {
            GameObject go = Instantiate(tempBossMonster);
            go.name = info.Name;
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
}