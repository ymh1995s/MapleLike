using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SpawnManager : MonoBehaviour
{
    private static SpawnManager instance;
    public static SpawnManager Instance { get { return instance; } }

    public GameObject damagePrefab;

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

    /// <summary>
    /// Addressable Group에서 key로 asset을 탐색해 Instantiate하는 메서드
    /// </summary>
    /// <param name="assetKey"></param>
    /// <returns></returns>
    public void SpawnAsset(string assetKey, Transform transform)
    {
        AsyncOperationHandle<GameObject> loadHandler;
        //loadHandler = Addressables.LoadAssetAsync<GameObject>(some asset);
        loadHandler = Addressables.LoadAssetAsync<GameObject>(assetKey);

        loadHandler.Completed += handle =>
        {
            if (loadHandler.Status == AsyncOperationStatus.Succeeded)
            {
                //Addressables.InstantiateAsync(loadHandler.Result);
                Instantiate(loadHandler.Result, transform);
            }
            else
            {
                Debug.LogError("Addressable Load Failed: " + assetKey);
            }
        };
    }


    /// <summary>
    /// 데미지를 화면에 띄우는 메서드
    /// </summary>
    /// <param name="damageList">연산이 끝난 데미지 리스트</param>
    /// <param name="target">공격 대상의 Transform</param>
    public void SpawnDamage(List<int> damageList, Transform target, bool isPlayerDamaged = false)
    {
        // 이거 SpawnManager의 SpawnAsset이랑 겹치는데;
        Vector3 spawnPosition = target.transform.position;

        float y = target.GetComponent<Collider2D>().bounds.max.y + 0.5f;
        spawnPosition.y = y;

        // TODO: 데미지 프리팹을 어디에 저장해둘지 (UI Manager로 예상)
        GameObject damage = Instantiate(damagePrefab, spawnPosition, Quaternion.identity);
        damage.GetComponentInChildren<DamageSpawner>().InitAndSpawnDamage(damageList, isPlayerDamaged);
    }
}
