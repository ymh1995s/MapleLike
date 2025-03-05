using Google.Protobuf.Protocol;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerAttackState : IPlayerState
{
    PlayerController playerController;
    public CurrentPlayerState playerState = CurrentPlayerState.AttackState;

    public PlayerAttackState(PlayerController player)
    {
        this.playerController = player;
    }

    public void Enter()
    {
        playerController.animator.SetTrigger("Attack");

        YHSMyPlayerController mpc = playerController as YHSMyPlayerController;
        if (mpc != null)
            mpc.SendPlayerMovePacket();
    }

    public void Execute()
    {
        // 점프 공격 시 위치 동기화 불가 문제 해결을 위해 추가
        // 근데 이게 맞나 싶음
        YHSMyPlayerController mpc = playerController as YHSMyPlayerController;
        if (mpc != null)
            mpc.SendPlayerMovePacket();
    }

    public void Exit()
    {
        // TODO: 점프 공격 시 피격당하면 원작과 다르게 공격이 중단됨
        // 점프 공격 시 공격이 중단되면 AttackState를 벗어나도록
        playerController.isAttacking = false;

        // 공격 후 숨찬 상태(HitState)로 진입하도록
        playerController.isDamaged = true;
    }

    public CurrentPlayerState ReturnNowState()
    {
        return playerState;
    }
}
