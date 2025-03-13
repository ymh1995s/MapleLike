using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StatusBarManager : MonoBehaviour
{
    [SerializeField] GameObject hpGauge;
    [SerializeField] GameObject mpGauge;
    [SerializeField] GameObject expGauge;
    [SerializeField] GameObject expEffect;

    private static StatusBarManager instance;
    public static StatusBarManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void InitStatusBar(PlayerInformation playerInformation)
    {
        playerInformation.UpdateHpAction += UpdateHpGauge;
        playerInformation.UpdateMpAction += UpdateMpGauge;
        playerInformation.UpdateExpAction += UpdateExpGauge;
    }

    public void UpdateHpGauge(float hp, float maxHp)
    {
        StartCoroutine(ScaleOverTime(hpGauge, hp / maxHp, 0.5f));
    }

    public void UpdateMpGauge(float mp, float maxMp)
    {
        StartCoroutine(ScaleOverTime(mpGauge, mp / maxMp, 0.5f));
    }

    public void UpdateExpGauge(float exp, float totalExp)
    {
        StartCoroutine(ScaleOverTime(expGauge, exp / totalExp, 0.5f));

        // TODO: 경험치바 끄트머리 반짝이 효과는 보류
        //Bounds bounds = expGauge.GetComponent<Renderer>().bounds;
        //float rightBound = bounds.max.x;
        //Vector3 effectPosition = expEffect.transform.position;
        //effectPosition.x = rightBound;
        //expEffect.transform.position = effectPosition;
    }

    IEnumerator ScaleOverTime(GameObject go, float targetScaleX, float time)
    {
        Vector3 startScale = go.transform.localScale;
        Vector3 targetScale = startScale;
        targetScale.x = targetScaleX;

        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            go.transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        go.transform.localScale = targetScale;
    }
}
