/*
 * 현재 구현은 시그너스 기준. 추후 보스가 추가되면 상속을 통해 확장 가능   
*/

using Google.Protobuf.Protocol;
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum BossMonsterState
{
    Idle = 0,
    Move = 1,
    Stun = 2,
    Skill = 3,
    Dead = 4,
}

enum BossMonsterSkill
{
    DarkGenesis = 0,
    DarknessBall = 1,
}

public class BossMonsterController : MonsterController
{
    // TODO: 보스 체력바 구현
    [SerializeField] GameObject hpBar;
    [SerializeField] Image hpBarGauge;

    // 패킷 수신 시 세팅되는 값
    BossMonsterState currentState = BossMonsterState.Idle;
    BossMonsterSkill currentSkill = BossMonsterSkill.DarkGenesis;
    bool isRight;

    // 스킬 발사체 스폰 포지션
    [SerializeField] GameObject darknessBallSpawnPosition;

    // 스킬 프리팹
    [SerializeField] GameObject darkGenesisSkillProjectilePrefab;
    [SerializeField] GameObject darknessBallSkillProjectilePrefab;

    protected override void FixedUpdate()
    {
        if (isAlreadyDie) return;

        // 위치 동기화 (SyncPos)
        base.FixedUpdate();

        // 변수 초기화
        if (currentState != BossMonsterState.Skill) hasUsedSkill = false;
        if (currentState != BossMonsterState.Stun) isAlreadyStun = false;
        
        switch (currentState)
        {
            case BossMonsterState.Idle:
                Idle();
                break;
            case BossMonsterState.Move:
                Move();
                break;
            case BossMonsterState.Stun:
                Stun();
                break;
            case BossMonsterState.Skill:
                Skill();
                break;
            case BossMonsterState.Dead:
                Dead();
                break;
        }
    }

    protected override void Idle()
    {
        AnimatorStateInfo animatorStateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
        if (!animatorStateInfo.IsName("idle")) monsterAnimator.SetTrigger("idle");
    }

    protected override void Move()
    {
        AnimatorStateInfo animatorStateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
        if (!animatorStateInfo.IsName("move")) monsterAnimator.SetTrigger("move");

        monsterSpriteRenderer.flipX = isRight ? true : false;
    }

    protected override void Stun()
    {
        if (isAlreadyStun) return;

        AnimatorStateInfo animatorStateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
        if (!animatorStateInfo.IsName("hit"))
        {
            monsterAnimator.SetTrigger("hit");
            isAlreadyStun = true;
        }
    }

    protected override void Skill()
    {
        if (hasUsedSkill) return;

        AnimatorStateInfo animatorStateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
        if (!animatorStateInfo.IsName("darkGenesis") && !animatorStateInfo.IsName("darknessBall"))
        {
            switch(currentSkill)
            {
                case BossMonsterSkill.DarkGenesis:
                    monsterAnimator.SetTrigger("darkGenesis");
                    break;
                case BossMonsterSkill.DarknessBall:
                    monsterAnimator.SetTrigger("darknessBall");
                    break;
            }
            hasUsedSkill = true;
        }
    }

    protected override void Dead()
    {
        AnimatorStateInfo animatorStateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
        if (!animatorStateInfo.IsName("die"))
        {
            monsterAnimator.SetTrigger("die");
            isAlreadyDie = true;
        }
    }

    // 패킷 수신 시 핸들러에서 호출. 서버에서 보낸 패킷에 따라 State 설정.
    public override void SetState(MonsterState newState)
    {
        switch (newState)
        {
            case MonsterState.MIdle:
                currentState = BossMonsterState.Idle;
                break;
            case MonsterState.MMoving:
                currentState = BossMonsterState.Move;
                break;
            case MonsterState.MStun:
                currentState = BossMonsterState.Stun;
                break;
            case MonsterState.MSkill:
                currentState = BossMonsterState.Skill;
                break;
            case MonsterState.MDead:
                currentState = BossMonsterState.Dead;
                break;
        }
    }

    // 패킷 수신 시 핸들러에서 호출. 서버에서 보낸 패킷에 따라 스프라이트 방향 설정.
    public override void SetDirection(bool isRight)
    {
        this.isRight = isRight;
    }

    // 패킷 수신 시 핸들러에서 호출. 서버에서 보낸 패킷에 따라 어떤 스킬을 사용할 지 결정.

    public void SetBossSkillType(BossMonsterSkillType newSkill)
    {
        currentSkill = (BossMonsterSkill)newSkill;
    }

    public void UpdateHPBarGauge()
    {
        // TODO: 보스 체력바 구현

        hpBar.SetActive(true);
        hpBarGauge.fillAmount = Mathf.Clamp(info.StatInfo.Hp / maxHp, 0.0f, 1.0f);
    }

    #region 스킬

    // attack1 애니메이션에서 이벤트로 호출됨.
    public void DarkGenesis()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            GameObject projectile = Instantiate(darkGenesisSkillProjectilePrefab);
            projectile.transform.position = player.transform.position;
        }
    }

    // attack2 애니메이션에서 이벤트로 호출됨.
    public void DarknessBall()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players) 
        {
            GameObject projectile = Instantiate(darknessBallSkillProjectilePrefab);
            Vector3 spawnPosition = darknessBallSpawnPosition.transform.position;

            // flipX에 따라 x 좌표를 반전
            spawnPosition.x *= monsterSpriteRenderer.flipX ? -1 : 1;

            projectile.transform.position = spawnPosition;
            projectile.GetComponent<DarknessBall>().target = player;
        }
    }
    #endregion
}