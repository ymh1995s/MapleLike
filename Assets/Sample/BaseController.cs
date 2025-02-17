using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    public int Id { get; set; }

    StatInfo _stat = new StatInfo();

    protected Vector3 destinationPos = new Vector3();

    protected virtual void Update()
    {
        SyncPos();
    }

    public void SyncPos()
    {
        // destinationPos : S_Move패킷을 통해 갱신된 목표 위치
        float distance = Vector3.Distance(transform.position, destinationPos);


        // 목표지점에 거의 다왔으면 목표 위치로 순간이동
        if (distance < 0.1f)
        {
            transform.position = destinationPos;
        }
        else
        {
            // 스르륵 이동
            transform.position = Vector3.MoveTowards(transform.position, destinationPos, 5 * Time.deltaTime);
        }
    }

    public void SetDestination(float x, float y)
    {
        destinationPos.x = x;
        destinationPos.y = y;
    }
}
