using UnityEngine;

public class PlayerIdleState : IPlayerState
{
    PlayerController playerController;
    public CurrentPlayerState playerState = CurrentPlayerState.IdleState;

    public PlayerIdleState(PlayerController player)
    {
        this.playerController = player;
    }

    public void Enter()
    {
        playerController.animator.SetTrigger("Idle");
        
        YHSMyPlayerController mpc = playerController as YHSMyPlayerController;
        if (mpc != null)
            mpc.SendPlayerMovePacket();
    }

    public void Execute()
    {
        
    }

    public void Exit()
    {

    }

    public CurrentPlayerState ReturnNowState()
    {
        return playerState;
    }
}
