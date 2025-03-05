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

        // 씬 로드
        //string temp = $"1_SnailPark";
        //SceneLoadManager.Instance.LoadScene(temp);

        // 임시
        enterPacket.PlayerInfo.PositionX = 0f;
        enterPacket.PlayerInfo.PositionY = 0f;

        SceneLoadManager.Instance.ExecuteOrQueue(() =>
        {
            ObjectManager.Instance.AddPlayer(enterPacket.PlayerInfo, myPlayer: true);
        });
    }

    public static void S_PlayerSpawnHandler(PacketSession session, IMessage packet)
    {
        S_PlayerSpawn spawnPacket = packet as S_PlayerSpawn;
        foreach (PlayerInfo obj in spawnPacket.PlayerInfos)
        {
            ObjectManager.Instance.AddPlayer(obj, myPlayer: false);
        }
    }

    /// <summary>
    /// 나를 제외한 타 플레이어 이동 동기화 메서드
    /// </summary>
    /// <param name="session"></param>
    /// <param name="packet"></param>
    public static void S_PlayerMoveHandler(PacketSession session, IMessage packet)
    {
        S_PlayerMove movePacket = packet as S_PlayerMove;

        GameObject go = ObjectManager.Instance.FindById(movePacket.PlayerId);
        if (go == null)
            return;

        // 내 플레이어는 클라이언트에서 자체 처리하므로 동기화 제외
        YHSMyPlayerController mpc = go.GetComponent<YHSMyPlayerController>();
        if (mpc != null)
            return;

        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null)
            return;

        pc.SetDestination(movePacket.PositionX, movePacket.PositionY);
        pc.SetPlayerDirection(movePacket.IsRight);
        pc.SetPlayerState(movePacket.State);
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
        // 플레이어의 Hit(Stun) 상태로의 전환은 S_PlayerMoveHandler()에서 처리한다.
        // 타 플레이어의 체력 동기화 부분으로 활용으로는 활용 가능할 듯.
    }

    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leavePacket = packet as S_LeaveGame;
        ObjectManager.Instance.Clear();

        // C_Die, C_ChangeMap를 서버가 수신 시, 자신이 사라지는 경우
        // C_ChangeMap을 다시 송신하거나 메인 화면으로 넘어가도록 해야?
    }

    public static void S_GetExpHandler(PacketSession session, IMessage packet)
    {
        S_LootItem leavePacket = packet as S_LootItem;
    }
}
