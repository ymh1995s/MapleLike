using Google.Protobuf.Protocol;
using UnityEngine;

// 몬스터 스킬을 MonsterSkill이라는 태그가 붙어야하며,
// 플레이어 측에서는 피해 처리 로직에서 이를 바탕으로 Collider, Trigger 처리를 해야함,

// TODO: 몬스터 스킬의 Collider는 플레이어의 공격, 몬스터, 몬스터 스킬이라는 태그, 레이어를 무시하도록 처리해야함.
public class MonsterSkill : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;
    protected Collider2D collider2D;

    // 플레이어가 몬스터 스킬과 충돌을 입었을 시, 해당 값을 바탕으로 체력 감소로직에 적용시켜야 함.
    protected int damage;
    
    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<Collider2D>();
    }

    // 애니메이션에서 이벤트로 호출, 애니메이션 종료 시 오브젝트가 Destroy 되도록
    public void DestroyEvent()
    {
        Destroy(gameObject);
    }
}
