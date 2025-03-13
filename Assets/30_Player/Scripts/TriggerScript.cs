using System.Collections.Generic;
using UnityEngine;

public class TriggerScript : MonoBehaviour
{
    Transform player;
    private List<MonsterController> Monsters;
    

    private void Awake()
    {
        player = transform.parent.parent;
        Monsters = new List<MonsterController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            Monsters.Add(collision.GetComponent<MonsterController>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            Monsters.Remove(collision.GetComponent<MonsterController>());
        }
    }

    // 해당 몬스터에게 데미지를 전달한다.
    // 횟수에 따라 아래 코드가 일정 주기로 반복되게 한다.

    /// <summary>
    /// 스킬이 발동됐을 때, 스킬 범위 내에 플레이어로부터 가장 가까운 몬스터를 찾는다.
    /// </summary>
    private MonsterController FindTarget()
    {
        Vector2 playerPos = player.position;
        if (Monsters.Count == 0)
        {
            return null;
        }
        MonsterController nearestMon = Monsters[0];

        for (int i = 0; i < Monsters.Count; i++)
        {
            if (Vector2.Distance(playerPos, Monsters[i].transform.position) < 
                Vector2.Distance(playerPos, nearestMon.transform.position)) 
            {
                nearestMon = Monsters[i];
            }
        }

        NormalMonsterController monster = nearestMon.GetComponent<MonsterController>() as NormalMonsterController;
        monster.SetState(Google.Protobuf.Protocol.MonsterState.MStun);

        return nearestMon;
    }

    public MonsterController GetTarget()
    {
        return FindTarget();
    }

    public List<MonsterController> GetAllTargets()
    {
        return Monsters;
    }
}
