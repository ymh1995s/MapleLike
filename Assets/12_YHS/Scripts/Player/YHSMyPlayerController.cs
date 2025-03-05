using Google.Protobuf.Protocol;
using UnityEngine;

public class YHSMyPlayerController : PlayerController
{
    // 플레이어 데이터 컴포넌트
    public PlayerInformation playerInformation;
    public PlayerInventory playerInventory;

    // 위치 계산에 필요한 컴포넌트
    private Rigidbody2D rb;
    private BoxCollider2D playerCollider;

    private bool isMyOnPortal = false;    // 플레이어가 포탈 위에 있는지

    private void Awake()
    {
        // My Player로써 키 입력에 따라 동작하기 위한 컴포넌트들 추가
        gameObject.AddComponent<InputManager>();

        playerInformation = gameObject.AddComponent<PlayerInformation>();
        playerInventory = gameObject.AddComponent<PlayerInventory>();

        rb = gameObject.AddComponent<Rigidbody2D>();
        playerCollider = gameObject.AddComponent<BoxCollider2D>();
    }

    protected override void Start()
    {
        // FSM 생성
        base.Start();
        playerInventory.Income = 1000;
        // 컴포넌트 내부 데이터 세팅
        rb.gravityScale = 3;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

        playerCollider.offset = new Vector2(0f, 0.7f);
        playerCollider.size = new Vector2(0.4f, 1.4f);
    }

    protected override void Update()
    {
        //base.Update();
    }

    protected override void FixedUpdate()
    {
        playerSM.Execute();

        if (isAttacking == false)
        {
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
        if (isDead == true)
        {
            OnDead();
            return;
        }

        // 플레이어 좌우 이동
        float horizontalDirection = Input.GetAxis("Horizontal");
        SetMyPlayerDirection(horizontalDirection);

        if (horizontalDirection != 0)
        {
            //Debug.Log("Key Pressed: " + horizontalDirection);

            if (IsGrounded() == true)
            {
                // jumpState일 때는 jumpState를 유지한다.
                rb.linearVelocityX = playerInformation.playerInfo.StatInfo.Speed * horizontalDirection;
                OnMove();
            }
        }
        else
        {
            if (IsGrounded() == true)
            {
                // jumpState일 때는 jumpState를 유지한다.
                if (isDamaged == true)
                {
                    OnHit();
                }
                else
                {
                    OnIdle();
                }
            }
        }

        // 플레이어 상하 이동
        if (Input.GetKey(KeyCode.UpArrow))
        {
            if (isMyOnPortal)
            {
                SendChangeMapPacket();
            }
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {

        }

        // 플레이어 점프
        if (IsGrounded() == true && Input.GetKey(KeyCode.LeftAlt))
        {
            //Debug.Log("Key Pressed: Left Alt");
            rb.linearVelocityY = playerInformation.playerInfo.StatInfo.Jump;
        }

        // 플레이어 공중부양 여부 판별
        if (IsGrounded() == false)
        {
            OnJump();
        }

        // 플레이어 공격
        if (Input.GetKey(KeyCode.LeftControl))
        {
            //Debug.Log("Key Pressed: Left Ctrl");
            OnAttack();
        }

        // 실질적인 위치 차이가 발생하면 이동 패킷을 전송한다.
        //Vector3 curPosition = transform.position;
        //float distance = (prevPosition - curPosition).magnitude;

        //if (distance > 0.01f)
        //{
        //    // 초당 송신 횟수를 지정하는 방법이 있다.
        //    // 필수적인 패킷은 즉시 전송하도록.
        //    SendMovePacket();
        //    prevPosition = curPosition;
        //}
    }

    #region 패킷 송신 기능
    /// <summary>
    /// 현재 위치, 방향, 상태를 서버에 송신한다.
    /// </summary>
    public void SendPlayerMovePacket()
    {
        C_PlayerMove movePacket = new C_PlayerMove();
        movePacket.State = (PlayerState)playerSM.NowState;
        movePacket.PositionX = transform.position.x;
        movePacket.PositionY = transform.position.y;
        movePacket.IsRight = isRight;

        NetworkManager.Instance.Send(movePacket);
    }

    /// <summary>
    /// 피격 사실을 서버에 송신한다.
    /// </summary>
    public void SendPlayerDamaged()
    {
        C_PlayerDamaged damagedPacket = new C_PlayerDamaged();
        NetworkManager.Instance.Send(damagedPacket);
    }

    /// <summary>
    /// 사망 상태를 서버에 송신한다.
    /// </summary>
    public void SendPlayerDiePacket()
    {
        C_PlayerDie diePacket = new C_PlayerDie();
        Debug.Log("Player Id [ " + Id + " ] has dead.");
        NetworkManager.Instance.Send(diePacket);
    }

    /// <summary>
    /// 이동할 맵의 데이터를 서버에 송신한다.
    /// </summary>
    void SendChangeMapPacket()
    {
        C_ChangeMap changeMapPacket = new C_ChangeMap();
        //changeMapPacket.mapId = 1234;
        NetworkManager.Instance.Send(changeMapPacket);
    }
    #endregion

    #region 패킷 수신 후 기능
    /// <summary>
    /// 서버로부터 S_EnterGame 수신 시 처리
    /// </summary>
    void SetInitPosition()
    {

    }
    #endregion

    /// <summary>
    /// 플레이어가 지형에 발을 붙이고 있는지 판별하는 메서드
    /// </summary>
    public bool IsGrounded()
    {
        float minBound = playerCollider.bounds.min.y;

        Vector2 groundCheckPosition = new Vector2(playerCollider.transform.position.x, minBound);
        float groundCheckRadius = 0.05f;

        return Physics2D.OverlapCircle(groundCheckPosition, groundCheckRadius, LayerMask.GetMask("Terrain"));
    }

    /// <summary>
    /// 내 플레이어가 바라보는 방향을 전환하는 메서드
    /// </summary>
    private void SetMyPlayerDirection(float moveDirection)
    {
        Vector3 scale = transform.localScale;

        if (moveDirection * scale.x > 0)
        {
            scale.x *= -1;
            transform.localScale = scale;
            isRight = !isRight;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Monster"))
        {
            SendPlayerDamaged();
            OnHit();

            // collision -> monster info -> calculate damage
            playerInformation.SetPlayerHp(-10);
        }

        // TODO: 포탈이 collider 아닌 trigger인지 확인 필요
        if (collision.collider.CompareTag("Portal"))
        {
            isMyOnPortal = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Monster"))
        {

        }

        // TODO: 포탈이 is trigger 활성화인지 확인 필요
        if (collision.collider.CompareTag("Portal"))
        {
            isMyOnPortal = false;
        }
    }
}