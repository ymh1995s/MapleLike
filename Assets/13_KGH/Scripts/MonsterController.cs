using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class MonsterController : BaseController
{
    protected MonsterInfo info = new MonsterInfo();
    public void UpdateInfo(MonsterInfo newInfo) => info = newInfo;

    protected Rigidbody2D monsterRigidbody;
    protected Collider2D monsterCollider;
    protected Animator monsterAnimator;
    protected SpriteRenderer monsterSpriteRenderer;

    // 지속적으로 서버로부터 넘어오는 State 변경 관련 패킷에 대해 애니메이션 동기화를 위한 변수
    protected bool hasUsedSkill = false;                          
    protected bool isAlreadyDie = false;
    protected bool isAlreadyStun = false;

    protected void Start()
    {
        monsterRigidbody = GetComponent<Rigidbody2D>();
        monsterCollider = GetComponent<Collider2D>();
        monsterAnimator = GetComponent<Animator>();
        monsterSpriteRenderer = GetComponent<SpriteRenderer>();
    }


    //======================================================================================================
    // 자식클래스의 구조에 따라 재정의가 필요한 함수.
    //====================================================================================================== 
    
    protected virtual void Idle() { }
    protected virtual void Move() { }
    protected virtual void Stun() { }
    protected virtual void Skill() { }
    protected virtual void Dead() { }
    public virtual void SetState(MonsterState newState) { }
    public virtual void SetDirection(bool isRight) { }

    //======================================================================================================
    // 자식클래스에서 구조 변경이 필요없음. 공통적으로 사용되는 함수. 재정의 필요없음
    //====================================================================================================== 

    // 몬스터의 Die 애니메이션의 끝에서 호출되는 이벤트 함수
    private void Despawn()
    {
        StartCoroutine(DespawnCoroutine());
    }

    private IEnumerator DespawnCoroutine()
    {
        monsterCollider.enabled = false;
        monsterRigidbody.bodyType = RigidbodyType2D.Kinematic;

        float duration = 1.5f; // 지속 시간
        float elapsedTime = 0f; // 경과 시간

        Color startColor = monsterSpriteRenderer.color; // 시작 색상
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0); // 끝 색상 (Alpha 0)

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime; // 경과 시간 업데이트
            monsterSpriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / duration); // 색상 보간
            yield return null; // 다음 프레임까지 대기
        }

        monsterSpriteRenderer.color = endColor; // 최종 색상 설정
        Destroy(gameObject);
    }
}
