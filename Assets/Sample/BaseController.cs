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
        // destinationPos : S_Move��Ŷ�� ���� ���ŵ� ��ǥ ��ġ
        float distance = Vector3.Distance(transform.position, destinationPos);


        // ��ǥ������ ���� �ٿ����� ��ǥ ��ġ�� �����̵�
        if (distance < 0.1f)
        {
            transform.position = destinationPos;
        }
        else
        {
            // ������ �̵�
            transform.position = Vector3.MoveTowards(transform.position, destinationPos, 5 * Time.deltaTime);
        }
    }

    public void SetDestination(float x, float y)
    {
        destinationPos.x = x;
        destinationPos.y = y;
    }
}
