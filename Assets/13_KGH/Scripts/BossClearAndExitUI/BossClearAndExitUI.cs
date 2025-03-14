using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BossClearAndExitUI : MonoBehaviour
{
    public static BossClearAndExitUI Instance;

    [SerializeField] GameObject bossClearLogo;
    [SerializeField] GameObject bossClearTooltip;
    [SerializeField] GameObject portalToViliage;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);
    }

    public void ExitButtonPressed()
    {
        C_ChangeMap changeMapPacket = new C_ChangeMap();
        changeMapPacket.MapId = (int)MapName.BossWaitRoom;
        NetworkManager.Instance.Send(changeMapPacket);
    }

    public void BossClear()
    {
        bossClearLogo.SetActive(true);
        StartCoroutine(BossClearLogoInactiveCoroutine());
        
        bossClearTooltip.SetActive(true);
        portalToViliage.SetActive(true);
    }

    private IEnumerator BossClearLogoInactiveCoroutine()
    {
        yield return new WaitForSeconds(5.0f);

        float duration = 5.0f;
        float elapsedTime = 0.0f;

        Color startColor = bossClearLogo.GetComponent<Image>().color;
        Color endColor = new Color(startColor.r, startColor.g, 0);

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            bossClearLogo.GetComponent<Image>().color = Color.Lerp(startColor, endColor, elapsedTime / duration);
            yield return null;
        }

        bossClearLogo.GetComponent<Image>().color = endColor;
        bossClearLogo.SetActive(false);
    }
}
