using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;

public class Archer : BaseClass
{
    [SerializeField] private GameObject arrowPrefab;
    protected override void Start()
    {
        base.Start();
        InitializeSkill();
        ClassStat();
    }

    /// <summary>
    /// 궁수 직업의 고유 특성 부여
    /// </summary>
    protected override void ClassStat()
    {
        playerStatInfo.MaxHp = (int)(playerStatInfo.MaxHp * 0.9f);
        playerStatInfo.MaxMp = (int)(playerStatInfo.MaxMp * 1.1f);
        playerStatInfo.ClassType = ClassType.Archer;

        playerStatInfo.Hp = playerStatInfo.Hp;
        playerStatInfo.Mp = playerStatInfo.Mp;
    }

    #region 히트박스 관련 코드
    public override void ActiveHitbox()
    {
        if (Hitbox != null)
        {
            Hitbox.enabled = true;
        }
        if (Effect != null)
        {
            Effect.SetActive(true);
            Effect.GetComponent<Animator>().SetTrigger("Attack");
            // 스킬 사용 이펙트 활성화 및 애니메이션 실행
        }
    }

    public override void DeactiveHitbox()
    {
        if (Hitbox != null)
        {
            Hitbox.enabled = false;
        }
        controller.isAttacking = false;
        controller.isDamaged = true;
    }

    public override void CreateHitEffect()
    {
        target = Hitbox.GetComponent<TriggerScript>().GetTarget();

        if (target != null)
        {
            // 플레이어가 공격했을 때 데미지를 화면에 띄운다.
            int totalDamage = 0;
            List<int> damageList = CalculatePlayerToMonsterDamage(
                target,
                playerStatInfo.AttackPower,
                "Double Shot",
                out totalDamage
                );
            SpawnManager.Instance.SpawnDamage(damageList, target.transform, false);

            SendHitMonsterPacket(totalDamage);
        }

        for (int i = 0; i < classSkill[0].hitCount; i++)
        {
            Invoke("CreateArrow", 0.01f);
        }
    }

    /// Todo
    /// 화살이 몬스터 방향으로 이동하며, 이동속도는 화살 활성화시간에 반비례. 화살은 0.5초간 날아가며,
    /// 최소 속도는 1f, 최대 속도는 3f
    /// 화살 오브젝트에 BoxCollider가 있고, 이 BoxCollider는 trigger임
    /// 화살 오브젝트의 trigger에 몬스터가 닿으면 파괴하고, 아래 주석처리된 hitGo 오브젝트를 생성하는 코드를 실행한다.
    /// 타겟이 없다면 플레이어가 바라보는 방향으로 5f만큼 날아가다 사라진다.
    /// <summary>
    /// 화살을 생성하여 타겟 방향으로 발사한다. 타겟이 없다면, 플레이어가 바라보는 방향으로 발사한다.
    /// </summary>
    private void CreateArrow()
    {
        GameObject arrowGo = Instantiate(arrowPrefab, 
                                        new Vector3(transform.parent.position.x + (-0.5f) * transform.parent.localScale.x, transform.parent.position.y + 0.4f),
                                        Quaternion.identity);
        arrowGo.transform.localScale = transform.parent.localScale;

        Vector3 start = arrowGo.transform.position;
        Vector3 end;
        if (target != null)
        {
            end = target.transform.position;
        }
        else
        {
            end = new Vector3(arrowGo.transform.position.x + (-5f) * arrowGo.transform.localScale.x, arrowGo.transform.position.y);
        }

        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        float minSpeed = 1f;
        float maxSpeed = 10f;
        float duration = 0.5f;
        float speed = Mathf.Clamp(distance / duration, minSpeed, maxSpeed);

        arrowGo.GetComponent<ArrowMovement>().Initialize(direction, speed, duration, HitObject, target);
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
            return true;
        }
    }
    #endregion
}
