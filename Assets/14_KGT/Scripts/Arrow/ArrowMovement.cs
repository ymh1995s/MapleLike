using UnityEngine;

public class ArrowMovement : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private float duration;
    private GameObject hitObject;
    private MonsterController monster;
    private float currentDuration;

    public void Initialize(Vector3 dir, float moveSpeed, float dur, GameObject hitObj, MonsterController mon = default)
    {
        currentDuration = 0f;
        direction = dir;
        speed = moveSpeed;
        duration = dur;
        hitObject = hitObj;
        if (mon != null)
        {
            monster = mon;
        }
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        currentDuration += Time.deltaTime;
        if (currentDuration > duration)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster") && collision.GetComponent<MonsterController>() == monster)
        {
            GameObject hitGo = Instantiate(hitObject, collision.transform.position, Quaternion.identity);
            hitGo.GetComponent<Animator>().SetTrigger("Hit");
            Destroy(hitGo, 0.45f);

            Destroy(gameObject);
        }
    }
}
