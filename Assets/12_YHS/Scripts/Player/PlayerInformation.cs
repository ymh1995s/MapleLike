using Google.Protobuf.Protocol;
using UnityEngine;
using System;

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

    public Action<float, float> UpdateHpAction;     // UI 동기화를 위한 Action
    public Action<float, float> UpdateMpAction;
    public Action<float, float> UpdateExpAction;

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

        float finalAttackPower = (mainAp * 4 + subAp) * 0.05f;
        //tempAttackPower *= 장비 공격력;

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

        playerStatInfo.MagicPower = initStatInfo.MagicPower + (int)((mainAp * 4 + subAp) * 0.05f);
    }

    private void CalculateDefense()
    {
        float defense = finalAp.Ap[(int)ApName.STR] * 4
                    + finalAp.Ap[(int)ApName.DEX] * 2
                    + finalAp.Ap[(int)ApName.INT] * 1;

        //defense += 장비 방어력;

        playerInfo.StatInfo.Defense = initStatInfo.Defense + (int)(defense * 0.1f);
    }

    private void CalculateHpMp()
    {
        // TODO:
    }
    #endregion

    #region 체력 관련 메서드
    public int GetPlayerHp()
    {
        return playerStatInfo.Hp;
    }

    public void SetPlayerHp(int changeAmount)
    {

        playerStatInfo.Hp += changeAmount;

        Debug.Log(changeAmount);
        int hp = playerStatInfo.Hp;
        int maxHp = playerStatInfo.MaxHp;

        if (hp > maxHp)
        {
            playerStatInfo.Hp = maxHp;
        }
        if (hp <= 0)
        {
            playerStatInfo.Hp = 0;
            hp = -1;
            playerController.OnDead();
        }
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
        playerStatInfo.Mp += changeAmount;

        Debug.Log(changeAmount);
        int mp = playerStatInfo.Mp;
        int maxMp = playerStatInfo.MaxMp;

        if (mp > maxMp)
        {
            playerStatInfo.Mp = maxMp;
        }
        if (mp < 0)
        {
            playerStatInfo.Mp = 0;
            mp = -1;
        }
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
        playerStatInfo.CurrentExp += changeAmount;

        Debug.Log(changeAmount);
        int exp = playerStatInfo.CurrentExp;
        int totalExp = playerStatInfo.TotalExp;

        if (exp >= totalExp)
        {
            playerStatInfo.CurrentExp = exp - totalExp;
            playerStatInfo.TotalExp = (int)(totalExp * 1.25f);     // 다음 레벨업에 필요한 경험치량 상승      
            exp -= totalExp;

            if (exp == 0)
            {
                exp = 1;
            }

            playerStatInfo.Level += 1;
            LevelUp();

            // 레벨업 애니메이션
            SpawnManager.Instance.SpawnAsset(ConstList.LevelUp, transform);
        }

        UpdateExpAction.Invoke(exp, totalExp);     // UI 동기화
        Debug.Log("EXP: " + exp + " / " + totalExp);
    }

    /// <summary>
    /// 레벨업에 따른 주스탯 자동 투자 메서드
    /// </summary>
    private void LevelUp()
    {
        ClassType classType = playerStatInfo.ClassType;

        switch (classType)
        {
            case ClassType.Warrior:
                playerAp.Ap[(int)ApName.STR] += 5;
                break;
            case ClassType.Magician:
                playerAp.Ap[(int)ApName.INT] += 5;
                break;
            case ClassType.Archer:
                playerAp.Ap[(int)ApName.DEX] += 5;
                break;
        }

        CalculateAp();
        CalculateAttackPower();
        CalculateMagicPower();
        CalculateDefense();
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
