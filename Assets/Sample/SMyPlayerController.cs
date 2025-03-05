using Google.Protobuf.Protocol;
using UnityEngine;

public class SMyPlayerController : SPlayerController
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
        if (Input.GetKeyDown(KeyCode.B))
        {
            C_ChangeMap changeMapPkt = new C_ChangeMap();
            changeMapPkt.MapId = 1;
            //changeMapPkt.spawnPoint = 0
            NetworkManager.Instance.Send(changeMapPkt);
        }
        if (Input.GetKeyDown(KeyCode.N))
        {
            C_ChangeMap changeMapPkt = new C_ChangeMap();
            changeMapPkt.MapId = 2;
            //changeMapPkt.spawnPoint = 0
            NetworkManager.Instance.Send(changeMapPkt);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            C_ChangeMap changeMapPkt = new C_ChangeMap();
            changeMapPkt.MapId = 3;
            //changeMapPkt.spawnPoint = 0
            NetworkManager.Instance.Send(changeMapPkt);
        }

        SendMovepacket(); // 매 프레임 송신하면 부하가 생기므로 적당히 보내야함 
    }

    void SendMovepacket()
    {
        C_PlayerMove movePacket = new C_PlayerMove();
        movePacket.PositionX = transform.position.x;
        movePacket.PositionY = transform.position.y;
        NetworkManager.Instance.Send(movePacket);
    }
}