
using Google.Protobuf.Protocol;
using UnityEngine;

public class PlayerController : BaseController
{
    // 플레이어 데이터 컴포넌트
    protected PlayerStateMachine playerSM;
    public Animator animator;
    GameObject character;
    BaseClass playerClass;

    public bool isRight = true;          // 플레이어가 바라보는 방향
    public bool isAttacking = false;     // 플레이어가 공격 중인지
    public bool isDamaged = false;       // 플레이어가 피격 되었는지
    public bool isDead = false;          // 플레이어가 사망했는지

    protected virtual void Awake()
    {
        character = transform.GetChild(0).gameObject;

        // FSM 초기화
        InitPlayerStateMachine();
        animator = character.GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        playerClass = character.GetComponent<BaseClass>();
    }

    private void InitPlayerStateMachine()
    {
        playerSM = new PlayerStateMachine(this);
        playerSM.Initialize(playerSM.idleState);
        playerSM.Initialize(playerSM.attackState);
        playerSM.Initialize(playerSM.moveState);
        playerSM.Initialize(playerSM.hitState);
        playerSM.Initialize(playerSM.deadState);
        playerSM.Initialize(playerSM.jumpState);
    }

    public void SetPlayerDirection(bool isRight)
    {
        // 플레이어의 방향 설정
        Vector3 scale = transform.localScale;

        if (this.isRight != isRight)
        {
            scale.x *= -1f;
            transform.localScale = scale;
            this.isRight = isRight;
        }
    }

    public void SetPlayerState(PlayerState currentState)
    {
        // 플레이어의 FSM 상태 설정
        if (playerSM == null)
        {
            InitPlayerStateMachine();
        }

        playerSM.TransitionByEnum(currentState);
    }

    #region FSM 상태 변환 메서드
    public void OnIdle()
    {
        if (playerSM.CurrentState != playerSM.idleState &&
            isAttacking == false)
        {
            playerSM.TransitionTo(playerSM.idleState);
        }
    }

    public void OnAttack()
    {
        if (playerSM.CurrentState != playerSM.attackState && playerClass.UseSkill())
        {
            isAttacking = true;
            playerSM.TransitionTo(playerSM.attackState);
        }
    }

    public void OnMove()
    {
        if (playerSM.CurrentState != playerSM.moveState)
        {
            playerSM.TransitionTo(playerSM.moveState);
        }
    }

    public void OnHit()
    {
        if (playerSM.CurrentState != playerSM.hitState && !isAttacking)
        {
            isDamaged = true;
            playerSM.TransitionTo(playerSM.hitState);
        }
    }

    public void OnDead()
    {
        if (playerSM.CurrentState != playerSM.deadState)
        {
            isDead = true;
            playerSM.TransitionTo(playerSM.deadState);
        }
    }

    public void OnJump()
    {
        if (playerSM.CurrentState != playerSM.jumpState)
        {
            playerSM.TransitionTo(playerSM.jumpState);
        }
    }
    #endregion
}