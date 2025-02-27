using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerContents.Object;
using ServerContents.Room;
using ServerContents.Session;
using ServerCore;
using System.Numerics;

public partial class PacketHandler
{
    public static void C_PlayerMoveHandler(PacketSession session, IMessage packet)
    {
        C_PlayerMove movePacket = packet as C_PlayerMove;
        ClientSession clientSession = session as ClientSession;

        //Console.WriteLine($"C_Move ({movePacket.PosInfo.CurrentPosX}, {movePacket.PosInfo.CurrentPosY})");

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleMove, player, movePacket);
    }

    public static void C_PlayerDieHandler(PacketSession session, IMessage packet)
    {
        // 이 플레이어가 죽었다고 '알리기만' 한다.
        C_PlayerDie movePacket = packet as C_PlayerDie;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleDie, player, movePacket);
    }

    public static void C_PlayerSkillHandler(PacketSession session, IMessage packet)
    {
        // 이 플레이어가 스킬을 사용했다고 '알리기만' 한다.
        C_PlayerSkill movePacket = packet as C_PlayerSkill;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleSkill, player, movePacket);
    }


    public static void C_PlayerDamagedHandler(PacketSession session, IMessage packet)
    {
        // 이 플레이어가 공격 받았다고 '알리기만' 한다.
        C_PlayerDamaged movePacket = packet as C_PlayerDamaged;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleDamaged, player, movePacket);
    }

    public static void C_ChangeMapHandler(PacketSession session, IMessage packet)
    {
        // 이 플레이어가 공격 받았다고 '알리기만' 한다.
        C_ChangeMap movePacket = packet as C_ChangeMap;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.HandleChangeMap, player, movePacket);
    }
}