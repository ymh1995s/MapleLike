using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using Unity.VisualScripting;
using UnityEngine;

// 플레이어 관련 패킷 핸들러
// 아마 기태님 현승님 작업하실 곳.
// 충돌이 우려되면 또 파셜로 나누면 됩니다.
public partial class PacketHandler
{
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterPacket = packet as S_EnterGame;
        ObjectManager.Instance.AddPlayer(enterPacket.PlayerInfo, myPlayer: true);
    }

    public static void S_PlayerSpawnHandler(PacketSession session, IMessage packet)
    {
        S_PlayerSpawn spawnPacket = packet as S_PlayerSpawn;
        foreach (PlayerInfo obj in spawnPacket.PlayerInfos)
        {
            ObjectManager.Instance.AddPlayer(obj, myPlayer: false);
        }
    }

    public static void S_PlayerMoveHandler(PacketSession session, IMessage packet)
    {
        S_PlayerMove movePacket = packet as S_PlayerMove;

        GameObject go = ObjectManager.Instance.FindById(movePacket.PlayerId);
        if (go == null)
            return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;

        bc.SetDestination(movePacket.PositionX, movePacket.PositionY);
    }

    public static void S_PlayerDespawnHandler(PacketSession session, IMessage packet)
    {
        S_PlayerDespawn despawnPacket = packet as S_PlayerDespawn;
        foreach (int id in despawnPacket.PlayerIds)
        {
            ObjectManager.Instance.Remove(id);
        }
    }

    public static void S_PlayerSkillHandler(PacketSession session, IMessage packet)
    {

    }

    public static void S_PlayerDamagedHandler(PacketSession session, IMessage packet)
    {

    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame LeavePacket = packet as S_LeaveGame;
        ObjectManager.Instance.Clear();
    }
}
