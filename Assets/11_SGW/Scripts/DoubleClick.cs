using UnityEngine;
using UnityEngine.UI;

public class DoubleClick : MonoBehaviour
{
    public Button targetButton;
    private float clickTime = 0f;
    private float clickInterval = 0.3f;
    private int clickCount = 0;
    
    
    public enum InventoryType { Inventory, Shop,Equip } // 현재 UI 타입 구분
    public enum ItemType { Equipment, Consumable, Misc }// 아이템 타입 구분

    [Header("UI 타입")]
    public InventoryType currentInventoryType;

    
 
    
    void Start()
    {
        targetButton = GetComponent<Button>();
        targetButton.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        clickCount++;

        if (clickCount == 1)
        {
            clickTime = Time.time;
        }
        else if (clickCount == 2)
        {
            if (Time.time - clickTime <= clickInterval)
            {
                HandleDoubleClick();
            }
            clickCount = 0;
        }

        if (Time.time - clickTime > clickInterval)
        {
            clickCount = 0;
        }
    }

    void HandleDoubleClick()
    {
        //
        if (currentInventoryType == InventoryType.Inventory)
        {
            // var currentItem = transform.GetComponentInChildren<Slot>().CurrentItem;
            var inventoyItemSlot = transform.GetComponentInChildren<Slot>();
            if (inventoyItemSlot == null)
            {
                Debug.Log("없어요");
                return;
            }
            if (inventoyItemSlot.CurrentItem == null)
            {
                Debug.LogWarning("현재 슬롯에 아이템이 없습니다!");
                return;
            }
            
            //슬롯안에 들어있는 아이템 카테고리가 장비라면
            if (inventoyItemSlot.CurrentItem.category == Item.ItemCategory.Equipment)
            {
                PlayerEquip eqiup = transform.root.GetComponent<PlayerEquip>();
                if (eqiup ==null)
                {
                    Debug.Log("없음");
                    return;
                }
                
                //장착 시 수치 증가  변수명 넣어서 사용하면됨  statValue 사용
                int statValue = eqiup.EquipItem(inventoyItemSlot.CurrentItem ,inventoyItemSlot);
                
                
            
                
            }
            //슬롯안에 들어있는 아이템 카테고리가 소비라면 
            else if (inventoyItemSlot.CurrentItem.category == Item.ItemCategory.Consumable)
            {
                PlayerInventory inventory = transform.root.GetComponent<PlayerInventory>();
                inventory.UseItem(inventoyItemSlot.CurrentItem);
            }
            
        }
        else if (currentInventoryType == InventoryType.Shop)
        {
           var showUI= transform.parent.GetComponent<ShowUI>();
           //부모오브젝트를 가져와서  있다면 
           if (showUI.ownerType == Info.OwnerType.Shop)
           {
               showUI.BuyItem();
           }
           else if (showUI.ownerType == Info.OwnerType.Player)
           {
               showUI.SellItem();
           }
        }
        //장비 벗기
        else if (currentInventoryType == InventoryType.Equip)
        {
            EquipSlot currentEquipSlot = transform.GetComponentInChildren<EquipSlot>();
            PlayerInventory inventory = transform.root.GetComponent<PlayerInventory>();
            if (currentEquipSlot != null)
            {
                inventory.AddItem(currentEquipSlot.CurrentItem);
                if (currentEquipSlot.CurrentItem is Equipment eq)
                {
                    //벗기 일때 statValue 사용
                    int statValue = currentEquipSlot.CurrentItem.ItemType switch
                    {
                        Google.Protobuf.Protocol.ItemType.Armor or Google.Protobuf.Protocol.ItemType.Helmet or    Google.Protobuf.Protocol.ItemType.Boots => eq.defensePower,
                        Google.Protobuf.Protocol.ItemType.Arrow or    Google.Protobuf.Protocol.ItemType.Sword => eq.attackPower,
                        Google.Protobuf.Protocol.ItemType.Staff  => eq.magicPower,
                        _ => 0 // 기본값 (예: 방어력/공격력이 없는 경우)
                    };
                    Debug.Log("장비 벗기"+statValue);
                }
                currentEquipSlot._image.sprite = null;
                currentEquipSlot.CurrentItem = null;
            }
        }
      
    }
}
