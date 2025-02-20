using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using Unity.VisualScripting;
using UnityEngine;

// 몬스터 관련 패킷 핸들러
// 기환님이 작업하시는 곳
public partial class PacketHandler
{
    public static void S_MonsterSpawnHandler(PacketSession session, IMessage packet)
    {
        S_MonsterSpawn spawnPacket = packet as S_MonsterSpawn;
        foreach (MonsterInfo obj in spawnPacket.MonsterInfos)
        {
            ObjectManager.Instance.AddMonster(obj);
        }
    }

    public static void S_MonsterMoveHandler(PacketSession session, IMessage packet)
    {
        S_MonsterMove movePacket = packet as S_MonsterMove;

        GameObject go = ObjectManager.Instance.FindById(movePacket.MonsterId);
        if (go == null)
            return;

        BaseController bc = go.GetComponent<BaseController>();
        if (bc == null)
            return;

        bc.SetDestination(movePacket.DestinationX, movePacket.DestinationY);
    }

    public static void S_MonsterDespawnHandler(PacketSession session, IMessage packet)
    {
        S_MonsterDespawn despawnPacket = packet as S_MonsterDespawn;
        foreach (int id in despawnPacket.MonsterIds)
        {
            ObjectManager.Instance.Remove(id);
        }
    }

    public static void S_MonsterSkillHandler(PacketSession session, IMessage packet)
    {

    }

    public static void S_HitMonsterHandler(PacketSession session, IMessage packet)
    {

    }
}
