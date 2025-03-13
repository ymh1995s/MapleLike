using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class ItemPickup : MonoBehaviour
{
    
    public float attractSpeed = 5f;  // 아이템이 빨려가는 속도
    public Transform player;
    private bool isAttracting = false;

    
    void Update()
    {
        if (isAttracting)
        {
            // 플레이어에게 점진적으로 이동
            transform.position = Vector2.MoveTowards(transform.position, player.position, attractSpeed * Time.deltaTime);
            
            // 일정 거리 이내로 들어오면 줍기 처리
            if (Vector2.Distance(transform.position, player.position) < 0.5f)
            {
                PickUp();
            }
        }
    }
    
    /// <summary>
    /// 아이템이 플레이어에게 당기게끔 하는 함수 
    /// </summary>
    /// <param name="_player"></param>
    public void StartAttracting(Transform _player) // 외부에서 호출 가능
    {
        this.player = _player;
        isAttracting = true;
        Debug.Log("isAttracting");
    }
    /// <summary>
    /// 플레이어에게 도착했을때 하는 함수 
    /// </summary>
    void PickUp()
    {
        var ItemInfo = transform.GetComponentInChildren<InitItem>();
        if (ItemInfo != null)
        {
            Debug.Log(ItemInfo);
        }
        player.GetComponent<PlayerInventory>().AddItem(ItemInfo.Property);
        isAttracting = false;
        Addressables.ReleaseInstance(this.gameObject);
        Debug.Log("아이템 줍기!");
        
    }
    
    
}
