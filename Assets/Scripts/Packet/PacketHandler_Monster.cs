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

        {
            BaseController bc = go.GetComponent<BaseController>();
            if (bc == null)
                return;

            bc.SetDestination(movePacket.DestinationX, movePacket.DestinationY);
        }

        // 일반몬스터의 경우
        NormalMonsterController nmc = go.GetComponent<NormalMonsterController>();
        if (nmc != null)
        {
            nmc.SetDirection(movePacket.IsRight);
            nmc.SetState(movePacket.State);
        }
        
        // 보스몬스터의 경우
        BossMonsterController bmc = go.GetComponent<BossMonsterController>();  
        if (bmc != null)
        {
            bmc.SetDirection(movePacket.IsRight);
            bmc.SetState(movePacket.State);
        }
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
        S_MonsterSkill skillPacket = packet as S_MonsterSkill;

        GameObject go = ObjectManager.Instance.FindById(skillPacket.MonsterId);
        if (go == null)
            return;

        // 일반몬스터의 경우
        NormalMonsterController nmc = go.GetComponent<NormalMonsterController>();
        if (nmc != null)
        {
            nmc.SetNormonsterSkillType(skillPacket.SkillType);
            nmc.SetState(MonsterState.MSkill);
        }

        // 보스몬스터의 경우
        BossMonsterController bmc = go.GetComponent<BossMonsterController>();
        if (bmc != null)
        {
            bmc.SetBossSkillType(skillPacket.SkillType);
            bmc.SetState(MonsterState.MSkill);
        }
    }

    public static void S_HitMonsterHandler(PacketSession session, IMessage packet)
    {
        S_HitMonster hitPacket = packet as S_HitMonster;

        GameObject go = ObjectManager.Instance.FindById(hitPacket.MonsterId);
        if (go == null)
            return;

        // 일반몬스터의 경우
        NormalMonsterController nmc = go.GetComponent<NormalMonsterController>();
        if (nmc != null)
            nmc.SetState(MonsterState.MStun);

        // 보스몬스터의 경우
        BossMonsterController bmc = go.GetComponent<BossMonsterController>();
        if (bmc != null)
            bmc.SetState(MonsterState.MStun);
    }

    public static void S_BossRegisterDenyHandler(PacketSession session, IMessage packet)
    {
        S_BossRegisterDeny bossRegisterDenyPacket = packet as S_BossRegisterDeny;
    }

    public static void S_BossWaitingHandler(PacketSession session, IMessage packet)
    {
        S_BossWaiting bossWatingPacket = packet as S_BossWaiting;
    }

    public static void S_GameClearHandler(PacketSession session, IMessage packet)
    {
        S_GameClear gameClearPacket = packet as S_GameClear;
    }
}
