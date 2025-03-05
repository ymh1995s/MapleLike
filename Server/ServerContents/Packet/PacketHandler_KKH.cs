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
        C_HitMonster hitPacket = packet as C_HitMonster;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.MonsterSetTargetToPlayer, player, hitPacket.MonsterId);

        // TODO: 추후, C_HitMonster에 playerAttackPower가 들어오면 이를 기반으로
        // room.Push(room.MonsterHitAndSetTarget, player, hitPacket.MonsterId, hitPacket.playerAttackPower);
    }
}