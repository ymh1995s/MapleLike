using Google.Protobuf.Protocol;
using UnityEngine;

public class MyPlayerController : PlayerController
{
    protected override void Update()
    {
        base.Update();
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (Input.GetKey(KeyCode.A))
        {
            transform.position = transform.position + new Vector3(-0.1f, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.position = transform.position + new Vector3(0.1f, 0, 0);
        }

        SendMovepacket(); // 매 프레임 송신하면 부하가 생기므로 적당히 보내야함 
    }

    void SendMovepacket()
    {
        C_Move movePacket = new C_Move();
        movePacket.PosX = transform.position.x;
        movePacket.PosY = transform.position.y;
        NetworkManager.Instance.Send(movePacket);
    }
}