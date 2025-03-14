using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DeathManager : MonoBehaviour
{
    public static DeathManager Instance;
    [SerializeField] private GameObject DeathPopup;
    [SerializeField] private Image Timer;
    public YHSMyPlayerController player;
    private Coroutine reviveCoroutine;

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// 코루틴을 정지시키는 함수
    /// </summary>
    public void StopCount()
    {
        if (reviveCoroutine != null)
        {
            StopCoroutine(reviveCoroutine);
            reviveCoroutine = null;
        }
    }

    /// <summary>
    /// 죽었을 때 팝업을 활성화하는 함수
    /// </summary>
    public void ActiveDeathPopup()
    {
        StopCount();

        if (DeathPopup != null)
        {
            DeathPopup.SetActive(true);
        }

        reviveCoroutine = StartCoroutine(StartReviveCount());
    }

    /// <summary>
    /// 버튼이 눌렸을 때, 팝업을 비활성화하는 함수
    /// </summary>
    public void DeactiveDeathPopup()
    {
        if (DeathPopup != null)
        {
            DeathPopup.SetActive(false);
        }
        RevivePlayer();
    }

    /// <summary>
    /// 플레이어의 체력을 1%로 깎고, 마을로 복귀시키는 함수
    /// </summary>
    private void RevivePlayer()
    {
        if (reviveCoroutine != null)
        {
            StopCount();
        }

        player.isDead = false;
        player.OnIdle();

        int hp = PlayerInformation.playerStatInfo.MaxHp / 20;
        int mp = PlayerInformation.playerStatInfo.MaxMp / 20;
        player.playerInformation.SetPlayerHp(hp);
        player.playerInformation.SetPlayerMp(mp);

        player.SendPlayerToVillage();
    }

    /// <summary>
    /// 플레이어가 버튼을 누르지 않을 시 30초간 대기했다가 부활시키는 코루틴
    /// </summary>
    IEnumerator StartReviveCount()
    {
        float duration = 30f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.fixedDeltaTime;

            if (Timer != null)
            {
                Timer.fillAmount = time / duration;
            }
            yield return null;
        }

        DeactiveDeathPopup();
    }
}
