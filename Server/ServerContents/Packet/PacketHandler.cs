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

    }

    public static void C_PlayerSkillHandler(PacketSession session, IMessage packet)
    {

    }


    public static void C_PlayerDamagedHandler(PacketSession session, IMessage packet)
    {

    }

    public static void C_ChangeMapHandler(PacketSession session, IMessage packet)
    {

    }
}