using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerContents.Object;
using ServerContents.Room;
using ServerContents.Session;
using ServerCore;
using System.Collections.Generic;
using System.Numerics;

public partial class PacketHandler
{
    public static void C_HitMonsterHandler(PacketSession session, IMessage packet)
    {
        // 1. 이 패킷을 보낸 클라이언트가 누구인지 알아낸다.
        ClientSession clientSession = session as ClientSession;

        // 2. 그 세션의 플레이어와 룸(맵)을 추출한다.
        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        // 3. 몬스터 아이디 추출
        C_HitMonster tempPacket = packet as C_HitMonster;

        // 4. 몬스터가 플레이어를 타게팅할 수 있도록 Room 쓰레드로 넘겨준다.
        // Push 문법 : Push(작업할 함수명, 파라미터1, 파라미터 2...)
        room.Push(room.MonsterSetTargetToPlayer, player, tempPacket.MonsterId);

        // TODO 이 몬스터가 죽었는지 안죽었는지 확인하고
        // S_DtopItem이나 S_MOnsterDespawn 패킷이 적절히 뿌려지도록 추가해주세요.
    }
}