using System.Collections;
using UnityEngine;

public class PlayerDeadState : IPlayerState
{
    PlayerController playerController;
    public CurrentPlayerState playerState = CurrentPlayerState.DeadState;
    
    GameObject character;       // 플레이어가 사망했을 때 스프라이트를 끄기 위한 게임오브젝트
    GameObject rip;             // 비석 게임오브젝트
    Animator ripAnimator;       // 비석 애니메이터

    bool trigger = false;       // 1회성 코루틴 실행용 bool

    public PlayerDeadState(PlayerController pc)
    {
        playerController = pc;

        character = pc.transform.GetChild(0).gameObject;
        rip = pc.transform.GetChild(1).gameObject;
        ripAnimator = rip.GetComponent<Animator>();
    }

    public void Enter()
    {
        trigger = false;

        character.GetComponent<SpriteRenderer>().enabled = false;
        rip.SetActive(true);

        SetRIP();

        YHSMyPlayerController mpc = playerController as YHSMyPlayerController;
        if (mpc != null)
        {
            mpc.SendPlayerMovePacket();
            mpc.SendPlayerDiePacket();
        }
    }

    public void Execute()
    {
        if (!trigger)
        {
            trigger = true;
            playerController.StartCoroutine(FallRIP());
        }
    }

    public void Exit()
    {
        // 사망 후 부활 시의 후처리
        playerController.isDead = false;
        character.GetComponent<SpriteRenderer>().enabled = true;
        rip.SetActive(false);
        trigger = false;
    }

    /// <summary>
    /// 비석의 위치를 플레이어 머리 위, 카메라 시점 상단으로 변경한다.
    /// 또한 떨어지는 애니메이션이 적용되도록 한다.
    /// </summary>
    private void SetRIP()
    {
        float y = Camera.main.orthographicSize + Camera.main.transform.position.y;
        rip.transform.position = new Vector3(playerController.gameObject.transform.position.x, y);
        ripAnimator.SetTrigger("Falling");
    }

    /// <summary>
    /// 비석이 떨어지고, 땅에 착지할 때까지의 작업
    /// </summary>
    IEnumerator FallRIP()
    {
        // 비석이 땅에 떨어질 때까지 기다린다.
        // 비석 오브젝트에 Rigidbody2D가 있으며, 본체 캐릭터 오브젝트와 레이어 제외를 각각해주었음.
        while (rip.transform.position.y > playerController.transform.position.y)
        {
            yield return null;
        }

        // 비석이 땅에 떨어진 후 착지 애니메이션 및 Idle로 이어지게 한다.
        ripAnimator.SetTrigger("Stand");

        // 좌표 동기화
        rip.transform.position = playerController.transform.position;

        yield break;
    }

    public CurrentPlayerState ReturnNowState()
    {
        return playerState;
    }
}
