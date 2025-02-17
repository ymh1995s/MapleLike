using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using Unity.VisualScripting;
using UnityEngine;

// 플레이어 관련 패킷 핸들러
public partial class PacketHandler
{
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterPacket = packet as S_EnterGame;
        ObjectManager.Instance.Add(enterPacket.Player, myPlayer: true);
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame LeavePacket = packet as S_LeaveGame;
        ObjectManager.Instance.Clear();
    }

    public static void S_DespawnHandler(PacketSession session, IMessage packet)
    {
        S_Despawn despawnPacket = packet as S_Despawn;
        foreach (int id in despawnPacket.ObjectIds)
        {
            ObjectManager.Instance.Remove(id);
        }
    }
    public static void S_SkillHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_ChangeHpHandler(PacketSession session, IMessage packet)
    {

    }
    public static void S_DieHandler(PacketSession session, IMessage packet)
    {

    }


    public static void S_MoveHandler(PacketSession session, IMessage packet)
    {
        S_Move movePacket = packet as S_Move;

        GameObject go = ObjectManager.Instance.FindById(movePacket.ObjectId);
        if (go == null)
            return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;

        bc.SetDestination(movePacket.PosX, movePacket.PosY);
    }

    public static void S_SpawnHandler(PacketSession session, IMessage packet)
    {
        S_Spawn spawnPacket = packet as S_Spawn;
        foreach (ObjectInfo obj in spawnPacket.Objects)
        {
            ObjectManager.Instance.Add(obj, myPlayer: false);
        }
    }

}
