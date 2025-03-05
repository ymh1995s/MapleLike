using Google.Protobuf.Protocol;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

#region 삭제 예정 구조체
public struct PlayerStatInformation
{
    // 나를 제외한 다른 플레이어는 이것을 가지고 있을 필요가 없을 것 같으므로
    // Protobuf 측 PlayerStatInfo로 대체
    public int level;           // 레벨
    public string className;    // 직업
    public int hp;              // 현재 체력
    public int maxHp;           // 최대 체력
    public int mp;              // 현재 마나
    public int maxMp;           // 최대 마나
    public int attackPower;     // 공격력
    public int magicPower;      // 마력
    public int defense;         // 방어력
    public float speed;         // 이동속도
    public float jump;          // 점프력
    public int currentExp;      // 현재 경험치
    public int totalExp;        // 목표 경험치
};
#endregion

public class PlayerInformation : MonoBehaviour
{
    #region 삭제 예정 멤버
    // Protobuf 측 PlayerInfo로 대체
    public int objectId;
    public string playerName;
    public float posX;
    public float posY;
    public PlayerStatInformation stats;
    #endregion

    // 아래 멤버만 남길 것이다.
    public PlayerController playerController;
    public PlayerInfo playerInfo;
    #region PlayerInfo 자료형 상세
    /*
    Google.Protobuf.Protocol.PlayerInfo
    {
        int PlayerId;
        string Name;
        float PositionX;
        float PositionY;
        PlayerStatInfo StatInfo;
        PlayerState CreatureState;
    }
    */
    #endregion
    #region PlayerStatInfo 자료형 상세
    /*
    Google.Protobuf.Protocol.PlayerStatInfo
    {
        int Level;
        string Class;
        int Hp;
        int MaxHp;
        int Mp;
        int MaxMp;
        int AttackPower;
        int MagicPower;
        int Defense;
        float Speed;
        float Jump;
        int CurrentExp;
        int TotalExp;
    }
    */
    #endregion

    private PlayerStatInfo statInfoTemp;
    private int playerLevel = 1;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    /// <summary>
    /// 게임 접속 시 PlayerInfo 초기화
    /// </summary>
    public void InitPlayerInfo(PlayerInfo info)
    {
        #region 삭제 예정 코드
        stats = new PlayerStatInformation()
        {
            // TODO: 직업별 스탯이 저장된 json으로부터 불러오도록 추후 변경
            level = 1,
            className = info.StatInfo.Class,
            hp = 50,    
            maxHp = 50,
            mp = 25,
            maxMp = 25,
            attackPower = 10,
            magicPower = 10,
            defense = 10,
            speed = 3,
            jump = 10,
            currentExp = 0,
            totalExp = 100,
        };
        #endregion

        // 아래 코드만 남길 것이다.
        PlayerStatInfo statInfo = new PlayerStatInfo();
        statInfoTemp = new PlayerStatInfo()
        {
            // JSON 파일 읽기에 실패하면 들어갈 임시 스탯 데이터
            Level = 0,
            Class = "Beginner",
            Hp = 10,
            MaxHp = 10,
            Mp = 10,
            MaxMp = 10,
            AttackPower = 1,
            MagicPower = 1,
            Defense = 100,
            Speed = 3,
            Jump = 10,
            CurrentExp = 0,
            TotalExp = 99999999,
        };

        //SetPlayerStat(ref statInfo);

        playerInfo = new PlayerInfo()
        {
            PlayerId = info.PlayerId,
            Name = info.Name,
            PositionX = info.PositionX,
            PositionY = info.PositionY,
            StatInfo = statInfoTemp,    // 패킷으로 받는 값: info.StatInfo,
            CreatureState = info.CreatureState, // fsm은 이미 관리주체가 있는데...
        };
    }

    #region 스탯 관련 메서드
    /// <summary>
    /// 기 설정된 스탯 값들이 담긴 JSON 파일로부터 스탯을 불러오는 메서드
    /// </summary>
    /// <param name="statInfo"></param>
    public void SetPlayerStat(ref PlayerStatInfo statInfo)
    {
        List<PlayerStatInfo> statInfoList;

        string filePath = Application.streamingAssetsPath + "/stats.json";  // 임시 경로

        SpawnManager.Instance.jsonFilePath.text = filePath;

        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            statInfoList = JsonConvert.DeserializeObject<List<PlayerStatInfo>>(json);

            foreach (var statInfoJson in statInfoList)
            {
                if (statInfoJson.Level == playerLevel)
                {
                    statInfo = statInfoJson;
                    return;
                }
            }
        }
        else
        {
            Debug.Log("Fail to open file");
            statInfo = statInfoTemp;
        }
    }
    #endregion

    #region 체력 관련 메서드
    public int GetPlayerHp()
    {
        return playerInfo.StatInfo.Hp;
    }

    public void SetPlayerHp(int changeAmount)
    {

        playerInfo.StatInfo.Hp += changeAmount;

        Debug.Log(changeAmount);
        int hp = playerInfo.StatInfo.Hp;
        int maxHp = playerInfo.StatInfo.MaxHp;

        if (hp > maxHp)
        {
            playerInfo.StatInfo.Hp = maxHp;
        }
        if (hp <= 0)
        {
            playerInfo.StatInfo.Hp = 0;
            playerController.SetPlayerState(PlayerState.PDead);
        }
        Debug.Log("HP: " + hp + " / " + maxHp);
    }
    #endregion

    #region 마나 관련 메서드
    public int GetPlayerMp()
    {
        return playerInfo.StatInfo.Mp;
    }

    public void SetPlayerMp(int changeAmount)
    {
        playerInfo.StatInfo.Mp += changeAmount;

        Debug.Log(changeAmount);
        int mp = playerInfo.StatInfo.Mp;
        int maxMp = playerInfo.StatInfo.MaxMp;

        if (mp > maxMp)
        {
            playerInfo.StatInfo.Mp = maxMp;
        }
        if (mp < 0)
        {
            playerInfo.StatInfo.Mp = 0;
        }
        Debug.Log("MP: " + mp + " / " + maxMp);
    }
    #endregion
}
