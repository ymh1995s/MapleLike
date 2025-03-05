using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InputManager : MonoBehaviour
{
    PlayerInformation playerInfo;

    // InputManager에서 이동/공격 조작까지 관리할지???
    private void Start()
    {
        playerInfo = GetComponent<PlayerInformation>();
    }

    private void FixedUpdate()
    {
        // FSM 상태 변환이 수반되지 않는 인풋 처리는 이곳에서 합니다.

        if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("ObjectId: " + playerInfo.playerInfo.PlayerId);
            Debug.Log("PlayerName: " + playerInfo.playerInfo.Name);
            Debug.Log("Level: " + playerInfo.playerInfo.StatInfo.Level);
            Debug.Log("Class: " + playerInfo.playerInfo.StatInfo.Class);
        }
        if (Input.GetKey(KeyCode.Z))
        {
            //Debug.Log("Key Pressed: Z"); 
            Debug.Log("GetItem");
        }
        if (Input.GetKey(KeyCode.PageUp))
        {
            //Debug.Log("Key Pressed: PgUp");
            Debug.Log("UseItem HP");
        }
        if (Input.GetKey(KeyCode.PageDown))
        {
            //Debug.Log("Key Pressed: PgDn");
            Debug.Log("UseItem MP");
        }

        // 테스트를 위한 임시 단축키들
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 체력 깎기
            playerInfo.SetPlayerHp(-10);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            // 씬 리로드
            SceneManager.LoadScene("LHSSampleScene");
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            C_ChangeMap changeMapPkt = new C_ChangeMap();
            changeMapPkt.MapId = 1;
            //changeMapPkt.spawnPoint = 0
            NetworkManager.Instance.Send(changeMapPkt);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            C_ChangeMap changeMapPkt = new C_ChangeMap();
            changeMapPkt.MapId = 2;
            //changeMapPkt.spawnPoint = 0
            NetworkManager.Instance.Send(changeMapPkt);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            C_ChangeMap changeMapPkt = new C_ChangeMap();
            changeMapPkt.MapId = 3;
            //changeMapPkt.spawnPoint = 0
            NetworkManager.Instance.Send(changeMapPkt);
        }
    }
}
