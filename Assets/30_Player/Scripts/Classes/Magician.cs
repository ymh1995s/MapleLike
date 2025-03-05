using Google.Protobuf.Protocol;
using UnityEngine;

public class Magician : BaseClass
{
    protected void Start()
    {
        base.Start();
        InitializeSkill();
        //ClassStat();
    }

    /// <summary>
    /// 법사 직업의 고유 특성 부여
    /// </summary>
    protected override void ClassStat()
    {
        info.stats.maxHp = (int)(info.stats.maxHp * 0.75f);
        info.stats.maxMp = (int)(info.stats.maxMp * 1.5f);
        info.stats.className = "Magician";

        info.stats.hp = info.stats.maxHp;
        info.stats.mp = info.stats.maxMp;
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
    }

    public override void CreateHitEffect()
    {
        target = Hitbox.GetComponent<TriggerScript>().GetTarget();

        if (target == null)
            return; 
        
        SendHitMonsterPacket();

        /// Todo
        /// 대상을 찾았기 때문에 클라이언트에서 몬스터에 데미지 처리를 한다.
        /// 또한 데미지 숫자 텍스트도 출력한다.

        GameObject hitGo = Instantiate(HitObject, target.transform.position, Quaternion.identity);
        hitGo.GetComponent<Animator>().SetTrigger("Hit");
        Destroy(hitGo, 0.45f);
    }
    #endregion

    #region 스킬
    /// <summary>
    /// 매직 클로
    /// </summary>
    public override bool UseSkill(MonsterStatInfo monster)
    {
        int cost = skillList["Magic Clow"].GetManaCost();
        if (info.stats.mp >= cost)
        {
            info.SetPlayerMp(cost);
            monster.Hp -= skillList["Magic Clow"].GetDamage();
            return true;
        }
        else
        {
            Debug.Log("MP 부족");
            return false;
        }
    }
    #endregion
}
