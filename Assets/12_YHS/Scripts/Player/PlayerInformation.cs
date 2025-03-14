using Google.Protobuf.Protocol;
using UnityEngine;
using System;
using Google.Protobuf.WellKnownTypes;

public class PlayerInformation : MonoBehaviour
{
    public PlayerController playerController;
    public static PlayerInfo playerInfo;            // 게임 실행 동안 유지되는 player info
    public static PlayerStatInfo playerStatInfo;    // 게임 실행 동안 유지되는 player stat info
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

    public static AbilityPoint playerAp = new AbilityPoint();           // 플레이어가 직접 투자한 AP
    public static AbilityPoint equipmentAp = new AbilityPoint();        // 장비 착용으로 상승하는 AP(임시)
    public static AbilityPoint finalAp = new AbilityPoint();            // 최종 AP

    public static PlayerStatInfo equipmentStat = new PlayerStatInfo();

    private PlayerStatInfo initStatInfo = new PlayerStatInfo()
    {
        // 임시 (확정일 수도 있는) 스탯 데이터
        // 아무것도 설정되지 않은 플레이어의 기본 스탯이다.
        Level = 1,
        ClassType = ClassType.Cnone,
        Hp = 100,
        MaxHp = 100,
        Mp = 100,
        MaxMp = 100,
        AttackPower = 10,
        MagicPower = 10,
        Defense = 10,
        Speed = 3,
        Jump = 10,
        CurrentExp = 0,
        TotalExp = 100,
    };

    public Action<int, int> UpdateHpAction;     // UI 동기화를 위한 Action
    public Action<int, int> UpdateMpAction;
    public Action<int, int> UpdateExpAction;
    public Action<int> UpdateLevelUpAction;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
    }

    /// <summary>
    /// 게임 접속 시 PlayerInfo 초기화
    /// </summary>
    public void InitPlayerInfo(PlayerInfo info)
    {
        playerInfo = new PlayerInfo()
        {
            PlayerId = info.PlayerId,
            Name = info.Name,
            PositionX = info.PositionX,
            PositionY = info.PositionY,
            StatInfo = playerStatInfo,              // 패킷으로 받는 값: info.StatInfo,
            CreatureState = info.CreatureState,     // fsm은 이미 관리주체가 있는데...
        };

        if (playerInfo.StatInfo == null)
        {
            // static 선언된 playerStatInfo는 최초 실행 시 한 번만 초기화한다.
            playerInfo.StatInfo = playerStatInfo = initStatInfo;
        }

        CalculateAp();
        CalculateAttackPower();
        CalculateMagicPower();
        CalculateDefense();
        CalculateHpMp();
    }

    #region 스탯 관련 메서드
    private void CalculateAp()
    {
        finalAp.Ap[(int)ApName.STR] = playerAp.Ap[(int)ApName.STR] + equipmentAp.Ap[(int)ApName.STR];
        finalAp.Ap[(int)ApName.DEX] = playerAp.Ap[(int)ApName.DEX] + equipmentAp.Ap[(int)ApName.DEX];
        finalAp.Ap[(int)ApName.INT] = playerAp.Ap[(int)ApName.INT] + equipmentAp.Ap[(int)ApName.INT];
        finalAp.Ap[(int)ApName.LUK] = playerAp.Ap[(int)ApName.LUK] + equipmentAp.Ap[(int)ApName.LUK];
    }

    private void CalculateAttackPower()
    {
        ClassType classType = playerStatInfo.ClassType;
        int mainAp = 1, subAp = 1;

        switch (classType)
        {
            case ClassType.Warrior:
                mainAp = playerAp.Ap[(int)ApName.STR];
                subAp = playerAp.Ap[(int)ApName.DEX];
                break;
            case ClassType.Magician:
                break;
            case ClassType.Archer:
                mainAp = playerAp.Ap[(int)ApName.DEX];
                subAp = playerAp.Ap[(int)ApName.STR];
                break;
            default:
                break;
        }

        float finalAttackPower = (mainAp * 4 + subAp) * equipmentStat.AttackPower * 0.05f;

        playerStatInfo.AttackPower = initStatInfo.AttackPower + (int)finalAttackPower;
    }

    private void CalculateMagicPower()
    {
        ClassType classType = playerStatInfo.ClassType;
        int mainAp = 1, subAp = 1;

        switch (classType)
        {
            case ClassType.Warrior:
                break;
            case ClassType.Magician:
                mainAp = playerAp.Ap[(int)ApName.INT];
                subAp = playerAp.Ap[(int)ApName.LUK];
                break;
            case ClassType.Archer:
                break;
            default:
                break;
        }

        float finalMagicPower = (mainAp * 4 + subAp) * equipmentStat.MagicPower * 0.05f;

        playerStatInfo.MagicPower = initStatInfo.MagicPower + (int)((mainAp * 4 + subAp) * 0.05f);
    }

    private void CalculateDefense()
    {
        float defense = finalAp.Ap[(int)ApName.STR] * 4
                    + finalAp.Ap[(int)ApName.DEX] * 2
                    + finalAp.Ap[(int)ApName.INT] * 1;

        defense += equipmentStat.Defense;

        playerInfo.StatInfo.Defense = initStatInfo.Defense + (int)(defense * 0.1f);
    }

    private void CalculateHpMp()
    {
        UpdateHpAction.Invoke(playerStatInfo.Hp, playerStatInfo.MaxHp);
        UpdateMpAction.Invoke(playerStatInfo.Mp, playerStatInfo.MaxMp);
    }
    #endregion

    #region 체력 관련 메서드
    public int GetPlayerHp()
    {
        return playerStatInfo.Hp;
    }

    public void SetPlayerHp(int changeAmount)
    {
        int hp = playerStatInfo.Hp + changeAmount;
        int maxHp = playerStatInfo.MaxHp;

        if (hp > maxHp)
        {
            hp = maxHp;
        }
        if (hp <= 0)
        {
            hp = 0;
            playerController.OnDead();
        }

        playerStatInfo.Hp = hp;
        UpdateHpAction.Invoke(hp, maxHp);      // UI 동기화
        Debug.Log("HP: " + hp + " / " + maxHp);
    }
    #endregion

    #region 마나 관련 메서드
    public int GetPlayerMp()
    {
        return playerStatInfo.Mp;
    }

    public void SetPlayerMp(int changeAmount)
    {
        int mp = playerStatInfo.Mp + changeAmount;
        int maxMp = playerStatInfo.MaxMp;

        if (mp > maxMp)
        {
            mp = maxMp;
        }
        if (mp < 0)
        {
            mp = 0;
        }

        playerStatInfo.Mp = mp;
        UpdateMpAction.Invoke(mp, maxMp);      // UI 동기화
        Debug.Log("MP: " + mp + " / " + maxMp);
    }
    #endregion

    #region 경험치 관련 메서드
    public int GetPlayerExp()
    {
        return playerStatInfo.CurrentExp;
    }

    public void SetPlayerExp(int changeAmount)
    {
        int exp = playerStatInfo.CurrentExp + changeAmount;
        int totalExp = playerStatInfo.TotalExp;

        if (exp >= totalExp)
        {
            exp -= totalExp;

            playerStatInfo.TotalExp = (int)(totalExp * 1.25f);     // 다음 레벨업에 필요한 경험치량 상승      
            playerStatInfo.Level += 1;
            UpdateLevelUpAction.Invoke(playerStatInfo.Level);

            // 레벨업 애니메이션
            SpawnManager.Instance.SpawnAsset(ConstList.LevelUp, transform);

            LevelUp();
        }

        playerStatInfo.CurrentExp = exp;
        UpdateExpAction.Invoke(exp, totalExp);     // UI 동기화
        Debug.Log("EXP: " + exp + " / " + totalExp);
    }

    /// <summary>
    /// 레벨업에 따른 스탯 자동 증가 메서드
    /// </summary>
    private void LevelUp()
    {
        ClassType classType = playerStatInfo.ClassType;

        int apIndex = 0;
        float hpExtendRate = 1f;
        float mpExtendRate = 1f;

        switch (classType)
        {
            case ClassType.Warrior:
                apIndex = (int)ApName.STR;
                hpExtendRate = 1.25f;
                mpExtendRate = 1.05f;
                break;
            case ClassType.Magician:
                apIndex = (int)ApName.STR;
                hpExtendRate = 1.10f;
                mpExtendRate = 1.35f;
                break;
            case ClassType.Archer:
                apIndex = (int)ApName.DEX;
                hpExtendRate = 1.15f;
                mpExtendRate = 1.15f;
                break;
        }

        playerAp.Ap[apIndex] += 5;
        playerStatInfo.MaxHp = (int)(playerStatInfo.MaxHp * hpExtendRate);
        playerStatInfo.MaxMp = (int)(playerStatInfo.MaxMp * mpExtendRate);

        CalculateAp();
        CalculateAttackPower();
        CalculateMagicPower();
        CalculateDefense();
        CalculateHpMp();

        // HP/MP 모두 회복
        SetPlayerHp(playerStatInfo.MaxHp);
        SetPlayerMp(playerStatInfo.MaxMp);
    }
    #endregion

    #region 테스트용 메서드
    public void PrintStatInfo()
    {
        Debug.Log("Level: " + playerStatInfo.Level);
        Debug.Log("Class: " + playerStatInfo.ClassType);
        Debug.Log("Hp: " + playerStatInfo.Hp);
        Debug.Log("MaxHp: " + playerStatInfo.MaxHp);
        Debug.Log("Mp: " + playerStatInfo.Mp);
        Debug.Log("MaxMp: " + playerStatInfo.MaxMp);
        Debug.Log("AttackPower: " + playerStatInfo.AttackPower);
        Debug.Log("MagicPower: " + playerStatInfo.MagicPower);
        Debug.Log("Defense: " + playerStatInfo.Defense);
        Debug.Log("Speed: " + playerStatInfo.Speed);
        Debug.Log("Jump: " + playerStatInfo.Jump);
        Debug.Log("CurExp: " + playerStatInfo.CurrentExp);
        Debug.Log("TotalExp: " + playerStatInfo.TotalExp);
    }

    #endregion
}
