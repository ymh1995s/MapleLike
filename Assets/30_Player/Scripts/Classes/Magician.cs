using Google.Protobuf.Protocol;
using System.Collections.Generic;
using UnityEngine;

public class Magician : BaseClass
{
    protected override void Start()
    {
        base.Start();
        InitializeSkill();
    }

    /// <summary>
    /// 법사 직업의 고유 특성 부여
    /// </summary>
    public override void ClassStat()
    {
        PlayerInformation.playerStatInfo.MaxHp = (int)(PlayerInformation.playerStatInfo.MaxHp * 0.75f);
        PlayerInformation.playerStatInfo.MaxMp = (int)(PlayerInformation.playerStatInfo.MaxMp * 3.5f);

        PlayerInformation.playerStatInfo.Hp = PlayerInformation.playerStatInfo.MaxHp;
        PlayerInformation.playerStatInfo.Mp = PlayerInformation.playerStatInfo.MaxMp;
    }

    #region 히트박스 관련 코드
    protected override void ActiveHitbox()
    {
        if (controller == null)
        {
            return;
        }

        if (currentHit != null)
        {
            Destroy(currentHit);
            currentHit = null;
        }

        Vector3 playerPosition = transform.parent.position;

        if (Hitbox != null)
        {
            currentHit = Instantiate(Hitbox, new Vector3(playerPosition.x + transform.parent.localScale.x * (-1.5f), playerPosition.y + 0.5f), Quaternion.identity);
            currentHit.GetComponent<TriggerScript>().player = controller.transform.position;
        }
    }

    protected void ActiveEffect()
    {
        if (Effect != null)
        {
            Effect.SetActive(true);
            Effect.GetComponent<Animator>().SetTrigger("Attack");
            // 스킬 사용 이펙트 활성화 및 애니메이션 실행
        }
    }

    public override void CreateHitEffect()
    {
        if (controller == null)
        {
            return;
        }

        target = currentHit.GetComponent<TriggerScript>().GetTarget();
        if (target == null)
            return;

        Vector3 targetPosition = target.GetComponent<BoxCollider2D>().bounds.center;

        // 플레이어가 공격했을 때 데미지를 화면에 띄운다.
        int totalDamage = 0;
        List<int> damageList = CalculatePlayerToMonsterDamage(
            target,
            playerStatInfo.MagicPower,    // 마법사는 마력을 사용한다.
            "Magic Clow",
            out totalDamage
            );
        SendHitMonsterPacket(damageList);

        GameObject hitGo = Instantiate(HitObject, targetPosition, Quaternion.identity);
        hitGo.GetComponent<Animator>().SetTrigger("Hit");
        Destroy(hitGo, 0.45f);
    }
    #endregion

    #region 스킬
    /// <summary>
    /// 매직 클로
    /// </summary>
    public override bool UseSkill()
    {
        int cost = skillList["Magic Clow"].GetManaCost();
        if (!CheckMana(cost))
        {
            Debug.Log("MP 부족");
            return false;
        }
        else
        {
            info.SetPlayerMp(-cost);
            SkillSource.PlayOneShot(skillSound["Magic Clow"]);
            return true;
        }
    }
    #endregion
}
