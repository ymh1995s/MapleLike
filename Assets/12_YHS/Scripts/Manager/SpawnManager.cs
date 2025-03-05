using UnityEngine;
using Google.Protobuf.Protocol;
using TMPro;

public class SpawnManager : MonoBehaviour
{
    private static SpawnManager instance;
    public static SpawnManager Instance { get { return instance; } }

    public TMP_Text jsonFilePath;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SpawnPlayer(PlayerInfo info, bool myPlayer = false)
    {
        // 플레이어의 직업에 맞는 캐릭터 프리팹을 생성해야 한다.
        // addressable을 사용하도록 하자.

        GameObject go = ObjectManager.Instance.FindById(info.PlayerId);

        go.transform.position = new Vector3(info.PositionX, info.PositionY, 0);

        if (myPlayer)
        {
            go.AddComponent<YHSMyPlayerController>();   // 조작을 위한 컴포넌트 부착

        }
        else
        {
            go.AddComponent<PlayerController>();        // 동기화를 위한 컴포넌트 부착

        }
    }
}
