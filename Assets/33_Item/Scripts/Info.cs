using TMPro;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// 상점 UI에 넣는 스크립트
/// </summary>
public class Info : MonoBehaviour
{
    
    public enum OwnerType
    {
        Player,  // 플레이어 소유
        Shop,    // 상점 소유
        NPC      // NPC가 소유 (예: 퀘스트 보상 아이템)
    }
    

    public int ID;
    public Image iconImage;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Price;
    
    
    
 
    /// <summary>
    /// 정보를 보여주는 함수 
    /// </summary>
    /// <param name="item">Item</param>
    /// <param name="ownerType">OwnerType</param>
    public void SetInfo(Item item, OwnerType ownerType)
    {
    
        iconImage.sprite = item.IconSprite;
        Name.text = item.itemName;
        ID = item.id;
        // 소유 주체에 따라 다른 가격 적용
        switch (ownerType)
        {
            case OwnerType.Player:
                Price.text = $"판매 가격: {item.sellprice} G";
                break;
            case OwnerType.Shop:
                Price.text = $"구매 가격: {item.buyprice} G";
                break;
            case OwnerType.NPC:
                Price.text = "획득 불가"; // NPC가 주는 퀘스트 보상 등
                break;
        }
    }

    public void SetInfo(Item item)
    {
        iconImage.sprite = item.IconSprite;
        Name.text = item.itemName;
        ID = item.id;
    }

}
