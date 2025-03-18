using Google.Protobuf.Protocol;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Archer : BaseClass
{
    [SerializeField] private GameObject arrowPrefab;
    
    protected override void Start()
    {
        base.Start();
        InitializeSkill();
    }

    /// <summary>
    /// 궁수 직업의 고유 특성 부여
    /// </summary>
    public override void ClassStat()
    {
        PlayerInformation.playerStatInfo.MaxHp = (int)(PlayerInformation.playerStatInfo.MaxHp * 0.9f);
        PlayerInformation.playerStatInfo.MaxMp = (int)(PlayerInformation.playerStatInfo.MaxMp * 2.5f);

        PlayerInformation.playerStatInfo.Hp = PlayerInformation.playerStatInfo.MaxHp;
        PlayerInformation.playerStatInfo.Mp = PlayerInformation.playerStatInfo.MaxMp;
    }

    #region 히트박스 관련 코드
    protected override void ActiveHitbox()
    {
        if (Effect != null)
        {
            Effect.SetActive(true);
            Effect.GetComponent<Animator>().SetTrigger("Attack");
            // 스킬 사용 이펙트 활성화 및 애니메이션 실행
        }

        //if (controller == null)
        //{
        //    return;
        //}

        if (currentHit != null)
        {
            Destroy(currentHit);
            currentHit = null;
        }

        Vector3 playerPosition = transform.parent.position;
        if (Hitbox != null)
        {
            currentHit = Instantiate(Hitbox, new Vector3(playerPosition.x + transform.parent.localScale.x * (-3.5f), playerPosition.y + 1f), Quaternion.identity);
            currentHit.GetComponent<TriggerScript>().player = playerPosition;
        }

    }

    public override void CreateHitEffect()
    {
        //if (controller == null)
        //{
        //    return;
        //}

        target = currentHit.GetComponent<TriggerScript>().GetTarget();

        for (int i = 0; i < classSkill[0].hitCount; i++)
        {
            CreateArrow(i);
        }
    }

    private void CreateDamageSkin()
    {
        if (controller == null)
        {
            return;
        }

        int totalDamage = 0;
        List<int> damageList = CalculatePlayerToMonsterDamage(
                                    target,
                                    playerStatInfo.AttackPower,
                                    "Double Shot",
                                    out totalDamage
                                    );
        SendHitMonsterPacket(damageList);
    }

    protected override List<int> CalculatePlayerToMonsterDamage(MonsterController target, int power, string skillKey, out int totalDamage)
    {
        List<int> damageList = new List<int>();
        totalDamage = 0;

        // 1레벨 스킬 데이터 가져오기
        float skillDamage = skillList[skillKey].skill.damage / 100f;
        int skillHitCount = skillList[skillKey].skill.hitCount;

        // 몬스터 방어력 가져오기
        float monsterDefense = 1 - target.info.StatInfo.Defense / 100f;

        for (int i = 0; i < skillHitCount; i++)
        {
            // 데미지 무작위 편차 부여
            float randomOffset = Random.Range(-0.5f, 0.5f);   // 0.5f 값을 추후에 "숙련도" 스탯으로 대체

            // 공식에 따라 데미지 계산
            int finalDamage = (int)((power * (1f + randomOffset)) * skillDamage * monsterDefense);
            damageList.Add(finalDamage);
            totalDamage += finalDamage;
        }

        return damageList;
    }

    /// <summary>
    /// 화살을 생성하여 타겟 방향으로 발사한다. 타겟이 없다면, 플레이어가 바라보는 방향으로 발사한다.
    /// </summary>
    private void CreateArrow(int y)
    {
        Collider2D hitCollider = currentHit.GetComponent<Collider2D>();
        Vector3 min = hitCollider.bounds.min;
        Vector3 max = hitCollider.bounds.max;
        Vector3 startPoint = GetComponentInParent<PlayerController>().isRight ? max : min;
        startPoint.y = hitCollider.bounds.center.y;

        GameObject arrowGo = Instantiate(arrowPrefab,
                                        new Vector3(
                                                        startPoint.x + (-0.5f) * transform.parent.localScale.x,
                                                        startPoint.y + (y * -0.1f) - 0.3f
                                                    ),
                                        Quaternion.identity);
        arrowGo.transform.localScale = transform.parent.localScale;
        arrowGo.GetComponent<ArrowMovement>().num = y;

        Vector3 start = arrowGo.transform.position;
        Vector3 end;
        if (target != null)
        {
            end = target.GetComponent<BoxCollider2D>().bounds.center;
        }
        else
        {
            end = new Vector3(arrowGo.transform.position.x + (-5f) * arrowGo.transform.localScale.x, arrowGo.transform.position.y);
        }

        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        float minSpeed = 10f;
        float maxSpeed = 20f;
        float duration = 1.5f;
        float speed = Mathf.Clamp(distance / duration, minSpeed, maxSpeed);

        arrowGo.GetComponent<ArrowMovement>().Initialize(direction, end, speed, duration, HitObject, target, CreateDamageSkin);
    }
    #endregion

    #region 스킬
    /// <summary>
    /// 더블 샷
    /// </summary>
    public override bool UseSkill()
    {
        int cost = skillList["Double Shot"].GetManaCost();
        if (!CheckMana(cost))
        {
            Debug.Log("MP 부족");
            return false;
        }
        else
        {
            info.SetPlayerMp(-cost);
            SkillSource.PlayOneShot(skillSound["Double Shot"]);
            return true;
        }
    }
    #endregion
}
