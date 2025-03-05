using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessBall : MonsterSkill
{
    [HideInInspector] public GameObject target;

    protected override void Start()
    { 
        base.Start();

        // TODO: 데미지 설정
        damage = 10;

        StartCoroutine(DestroyCoroutine());
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            spriteRenderer.flipX = transform.position.x > target.transform.position.x ? false : true;
            transform.position = Vector3.MoveTowards(transform.position, target.transform.position, Time.fixedDeltaTime * 1.5f);
        }
    }

    IEnumerator DestroyCoroutine()
    {
        // 5초 대기
        yield return new WaitForSeconds(5f);

        // 1에서 0으로 alpha 값을 서서히 변화
        float duration = 2.5f; // 7.5초까지의 시간
        float startAlpha = 1f;
        float endAlpha = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;

            yield return null; // 다음 프레임까지 대기
        }

        Color finalColor = spriteRenderer.color;
        finalColor.a = endAlpha;
        spriteRenderer.color = finalColor;

        Destroy(gameObject); 
    }
}
