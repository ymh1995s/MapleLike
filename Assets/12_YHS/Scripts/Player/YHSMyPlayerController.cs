using Google.Protobuf.Protocol;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class YHSMyPlayerController : PlayerController
{
    // 플레이어 데이터 컴포넌트
    public PlayerInformation playerInformation;
    public PlayerInventory playerInventory;
    public PlayerEquip playerEquip;

    // 위치 계산에 필요한 컴포넌트
    private Rigidbody2D rb;
    private BoxCollider2D playerDamagedCollider;    // 피격 판별용 (trigger)
    private EdgeCollider2D playerTerrainCollider;   // 지형 판별용

    // 기타 컴포넌트
    private CinemachineCamera cinemachine;

    private bool isMyOnPortal = false;    // 플레이어가 포탈 위에 있는지
    private bool isPressedJump = false;
    private MapName nextMapName;            // 이동할 맵 이름
    
    
    public Transform Inventory;
    public Transform Equipment;
    private GameObject player;

    protected override void Awake()
    {
        // FSM 생성
        base.Awake();

        // My Player로써 키 입력에 따라 동작하기 위한 컴포넌트들 추가
        gameObject.AddComponent<InputManager>();

        playerInformation = gameObject.AddComponent<PlayerInformation>();
        playerInventory = gameObject.AddComponent<PlayerInventory>();
        //03-07 추가
        playerEquip = gameObject.AddComponent<PlayerEquip>();
        
        rb = gameObject.AddComponent<Rigidbody2D>();
        playerDamagedCollider = gameObject.AddComponent<BoxCollider2D>();
        playerTerrainCollider = gameObject.AddComponent<EdgeCollider2D>();

        // 플레이어 스탯 UI 초기화
        //StatusBarManager sbm = FindFirstObjectByType<StatusBarManager>();
        StatusBarManager.Instance.InitStatusBar(playerInformation);

        // 사망 처리를 위한 UI 초기화
        DeathManager.Instance.player = this;

        // 카메라 초기화
        cinemachine = GameObject.Find("CinemachineCamera").GetComponent<CinemachineCamera>();

        if (cinemachine == null)
        {
            Debug.LogError("Can't find Cinemachine Camera");
        }
    }

    protected override void Start()
    {
        base.Start();

        rb.gravityScale = 3;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

        playerDamagedCollider.offset = new Vector2(0f, 0.7f);
        playerDamagedCollider.size = new Vector2(0.4f, 1.4f);
        playerDamagedCollider.isTrigger = true;
        playerTerrainCollider.points = new Vector2[] { new Vector2(0f, 0.01f), new Vector2(0f, 0f) };

        // 카메라 세팅
        cinemachine.Follow = gameObject.transform;
        cinemachine.LookAt = gameObject.transform;
        
        player =  ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
        Inventory = player.transform.Find("Character/Canvas/Inventory");
        Equipment = player.transform.Find("Character/Canvas/Equip");
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
                rb.linearVelocityX = PlayerInformation.playerStatInfo.Speed * horizontalDirection;
                OnMove();
            }
            else
            {
                OnJump();
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
            else
            {
                OnJump();
            }
        }

        // 플레이어 상하 이동
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (isMyOnPortal)
            {
                SendChangeMapPacket();
                // TODO: 맵 이동할 때 페이드인아웃 애니메이션
            }
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {

        }

        // 플레이어 점프
        if (IsGrounded() == true && Input.GetKey(KeyCode.LeftAlt))
        {
            //Debug.Log("Key Pressed: Left Alt");
            rb.linearVelocityY = PlayerInformation.playerStatInfo.Jump;
            isPressedJump = true;
        }

        // 플레이어 공중부양 여부 판별
        //if (IsGrounded() == false)
        //{
        //    OnJump();
        //}

        // 플레이어 공격
        if (Input.GetKey(KeyCode.LeftControl) && !isAttacking)
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
    private void SendChangeMapPacket()
    {
        C_ChangeMap changeMapPacket = new C_ChangeMap();
        changeMapPacket.MapId = (int)nextMapName;
        NetworkManager.Instance.Send(changeMapPacket);
    }
    #endregion

    #region 패킷 수신 후 기능
    
    #endregion

    #region 동작 관련 메서드
    /// <summary>
    /// 플레이어가 지형에 발을 붙이고 있는지 판별하는 메서드
    /// </summary>
    public bool IsGrounded()
    {
        Vector2 boxCenter = new Vector2(playerTerrainCollider.bounds.center.x, playerTerrainCollider.bounds.min.y - 0.05f);
        Vector2 boxSize = new Vector2(playerTerrainCollider.bounds.size.x, 0.1f);

        bool isOnGround = Physics2D.OverlapBox(boxCenter, boxSize, 0f, LayerMask.GetMask("Terrain"));

        if (isPressedJump == true)
        {
            if (rb.linearVelocityY <= 0)
            {
                isPressedJump = false;
            }
            return false;
        }

        return isOnGround;
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

    /// <summary>
    /// 플레이어를 마을로 보내는 메서드
    /// </summary>
    public void SendPlayerToVillage()
    {
        nextMapName = MapName.Village;
        SendChangeMapPacket();
    }
    #endregion

    #region 충돌 처리 관련 메서드
    /// <summary>
    /// 몬스터가 플레이어를 공격했을 때의 데미지를 계산하는 메서드
    /// </summary>
    /// <param name="target">공격당한 플레이어</param>
    /// <param name="power">몬스터 공격력</param>
    /// <param name="totalDamage">몬스터가 가한 총 데미지</param>
    private List<int> CalculateMonsterToPlayerDamage(MonsterController monster, out int totalDamage)
    {
        List<int> damageList = new List<int>();
        totalDamage = 0;

        // 몬스터 공격력 가져오기
        int monsterPower = monster.info.StatInfo.AttackPower;

        // 플레이어 방어력 가져오기
        int defense = PlayerInformation.playerStatInfo.Defense;

        int skillHitCount = 1;  // 우선은 1타만 맞도록 구현
        for (int i = 0; i < skillHitCount; i++)
        {
            // 데미지 무작위 편차 부여
            float randomOffset = Random.Range(-0.5f, 0.5f);   // 0.5f 값을 추후에 "숙련도" 스탯으로 대체

            // 공식에 따라 데미지 계산
            int finalDamage = monsterPower - defense;
            finalDamage = Mathf.Clamp(finalDamage, 0, finalDamage);

            damageList.Add(finalDamage);
            totalDamage += finalDamage;
        }

        return damageList;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

    }

    private void OnCollisionExit2D(Collision2D collision)
    {

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            MonsterController monster = collision.GetComponent<MonsterController>();

            // 플레이어가 공격받았을 때 데미지를 화면에 띄운다.
            int totalDamage = 0;
            List<int> damageList = CalculateMonsterToPlayerDamage(
                monster,
                out totalDamage
                );
            SpawnManager.Instance.SpawnDamage(damageList, transform, true);

            playerInformation.SetPlayerHp(-totalDamage);
            SendPlayerDamaged();
            OnHit();
        }

        if (collision.CompareTag("Portal"))
        {
            isMyOnPortal = true;
            nextMapName = collision.gameObject.GetComponent<Portal>().nextMapName;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Portal"))
        {
            isMyOnPortal = false;
        }
    }
    #endregion
}