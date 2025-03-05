using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 직업 요소를 담는 추상 클래스
/// 캐릭터의 스킬 타격범위 트리거를 활성화하는 역할도 지니고 있다.
/// 트리거를 활성화, 비활성화하는 함수와 공격이 끝난 후 컨트롤러에서 공격상태가 종료됨을 알리는 함수를 가짐
/// 이 함수들은 애니메이션 내에 이벤트로 지정되어 있다.
/// </summary>
public abstract class BaseClass : MonoBehaviour
{
    protected PlayerInformation info;
    //protected Controller controller;
    protected YHSMyPlayerController controller;

    [Header("스킬 요소")]
    public Dictionary<string, SkillData> skillList = new Dictionary<string, SkillData>();    // 해당 직업의 스킬과 현재 스킬 레벨
    public Skill[] classSkill;    // 해당 직업의 스킬
    public Collider2D Hitbox;
    public GameObject Effect;
    public GameObject HitObject;

    protected GameObject target;

    protected void Awake()
    {
        info = GetComponent<PlayerInformation>();
        //controller = GetComponentInParent<Controller>();
        controller = GetComponentInParent<YHSMyPlayerController>();
    }

    protected void Start()
    {
        info = GetComponent<PlayerInformation>();
        controller = GetComponentInParent<YHSMyPlayerController>();
    }

    /// <summary>
    /// 직업의 스펙을 조정한다.
    /// </summary>
    protected abstract void ClassStat();

    /// <summary>
    /// 스킬을 사용할 때 사용한다. Bool타입으로 스킬 사용에 성공했는지 여부를 리턴한다.
    /// </summary>
    public abstract bool UseSkill(MonsterStatInfo monster);

    /// <summary>
    /// 해당 직업의 스킬을 초기화한다.
    /// </summary>
    protected void InitializeSkill()
    {
        if (classSkill == null) { return; }
        for (int i = 0; i < classSkill.Length; i++)
        {
            skillList[classSkill[i].skillName] = new SkillData(classSkill[i], 1);
        }
    }

    /// <summary>
    /// 스킬포인트를 이용해 스킬 레벨을 올릴 때 사용한다.
    /// </summary>
    protected bool PointUpSkill(string skillName)
    {
        if (!skillList.ContainsKey(skillName)) { return false; }
        if (skillList[skillName].level >= skillList[skillName].skill.maxLevel) { return false; }

        skillList[skillName].level++;
        return true;
    }

    public abstract void ActiveHitbox();

    /// <summary>
    /// 실제 타격 이펙트를 생성하는 함수. 데미지 처리 밑 패킷을 생성하여 보내는 처리도 여기서 할 예정
    /// 히트 오브젝트를 생성하는 함수
    /// 트리거 스크립트로부터 대상을 받아 해당 대상의 위치에 히트 오브젝트 생성
    /// (현재는 플레이어가 타격한 방향과 상관없이 생성중)
    /// </summary>
    public abstract void CreateHitEffect();

    protected void SendHitMonsterPacket()
    {
        C_HitMonster HitMonster = new C_HitMonster();
        HitMonster.MonsterId = target.GetComponent<MonsterStatInfo>().Id;
        // 몬스터에 입힌 데미지량도 추가한다.
        NetworkManager.Instance.Send(HitMonster);
    }

    public abstract void DeactiveHitbox();
}
