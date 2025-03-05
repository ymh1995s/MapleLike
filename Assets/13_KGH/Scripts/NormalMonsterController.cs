using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Rendering;

enum NormalMonsterState
{
    Idle = 0,
    Move = 1,
    Stun = 2,
    Skill = 3,
    Dead = 4,
}


// TODO: 현재 일반 몬스터의 스킬 구현 계획 없음.
enum NormalMonsterSkill
{
    Skill0 = 0,
    Skill1 = 1,
    // ...
}

public class NormalMonsterController : MonsterController
{
    // 패킷 수신 시 세팅되는 값
    NormalMonsterState currentState;
    NormalMonsterSkill currentSkill = NormalMonsterSkill.Skill0;    // TODO: 아직 일반 몬스터 스킬 구현 계획 없음.
    bool isRight;

    protected override void FixedUpdate()
    {
        if (isAlreadyDie) return;

        // 위치 동기화 (SyncPos)
        base.FixedUpdate();

        // 변수 초기화
        if (currentState != NormalMonsterState.Skill) hasUsedSkill = false;
        if (currentState != NormalMonsterState.Stun) isAlreadyStun = false;

        switch (currentState)
        {
            case NormalMonsterState.Idle:
                Idle();
                break;
            case NormalMonsterState.Move:
                Move();
                break;
            case NormalMonsterState.Stun:
                Stun();
                break;
            case NormalMonsterState.Skill:
                Skill();
                break;
            case NormalMonsterState.Dead:
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
        // TODO: 아직 일반 몬스터 스킬 구현 계획 없음.
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

    // 패킷 수신 시 핸들러에서 호출. 서버에서 보낸 패킷에 따라 State 설정
    public override void SetState(MonsterState newState)
    {
        switch (newState)
        {
            case MonsterState.MIdle:
                currentState = NormalMonsterState.Idle;
                break;
            case MonsterState.MMoving:
                currentState = NormalMonsterState.Move;
                break;
            case MonsterState.MStun:
                currentState = NormalMonsterState.Stun;
                break;
            case MonsterState.MSkill:
                currentState = NormalMonsterState.Skill;
                break;
            case MonsterState.MDead:
                currentState = NormalMonsterState.Dead;
                break;
        }
    }

    // 패킷 수신 시 핸들러에서 호출. 서버에서 보낸 패킷에 따라 스프라이트 방향 설정 
    public override void SetDirection(bool isRight)
    {
        this.isRight = isRight;
    }


    // TODO: 아직 일반 몬스터 스킬 구현 계획 없음.
    public void SetNormonsterSkillType(BossMonsterSkillType newSkill)
    {
        currentSkill = (NormalMonsterSkill)newSkill;
    }
}