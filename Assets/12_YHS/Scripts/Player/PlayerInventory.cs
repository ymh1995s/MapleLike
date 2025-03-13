using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class PlayerInventory : MonoBehaviour
{
    
    public List<Item> items = new List<Item>();
    
    //자식오브젝트에 들어있는 Slot 스크립트 전부 가져오기 
    public List<Slot> Slots = new List<Slot>();
    
    public TextMeshProUGUI TxtGold;
    
    public PlayerInventory ClientInventroy;
    
    
    //플레이어가 가지고 있는 돈을 확인
    public int Income;

    
    private void Start()
    {
        Slots = new List<Slot>(transform.GetComponentsInChildren<Slot>());
        var Player = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
        //게임 오브젝트 가져오기
        ClientInventroy = Player.GetComponent<YHSMyPlayerController>().playerInventory;
        TextMeshProUGUI[] textComponents = GetComponentsInChildren<TextMeshProUGUI>();
        foreach (var text in textComponents)
        {
            if (text.gameObject.name == "TxtGold")
            {
                TxtGold = text; // 정확한 TxtGold 찾기
                Debug.Log("🎯 정확한 TxtGold 찾음: " + TxtGold.text);
                break; // 찾았으면 더 이상 반복할 필요 없음
            }
        }

        Income = 10000;
        TxtGold.text = Income.ToString();
        Player.GetComponent<YHSMyPlayerController>().Inventory.gameObject.SetActive(false);
    }
    
    public void AddItem(Item newItem)
    {
        if (newItem == null)
        {
            return;
        }
        // 기존에 있는 아이템인지 확인
        Slot existingSlot = Slots.FirstOrDefault(slot => slot.CurrentItem != null && slot.CurrentItem.id == newItem.id);

        if (existingSlot != null)
        {
            // 🔥 이미 존재하는 아이템이면 개수 증가
            existingSlot.Count++;
            existingSlot.UpdateUI();
            Debug.Log($"🟢 {newItem.itemName} 개수 증가: {existingSlot.Count}");
        }
        else
        {
            // 🔥 새로운 아이템이면 빈 슬롯에 추가
            Slot emptySlot = Slots.FirstOrDefault(slot => slot.CurrentItem == null);
            if (emptySlot != null)
            {
                emptySlot.Count++;
                emptySlot.SetItem(newItem);
                emptySlot.UpdateUI();
                Debug.Log($"🟢 {newItem.itemName} 개수 증가: {emptySlot.Count}");
            }
            else
            {
                Debug.LogWarning("❌ 인벤토리에 빈 슬롯이 없습니다!");
            }
        }
    }

    /// <summary>
    /// 소비 아이템 사용
    /// </summary>
    /// <param name="newItem"></param>
    public void UseItem(Item newItem)
    {
        if (newItem == null) { return; }

        if (newItem.category == Item.ItemCategory.Equipment)
        {
            Debug.Log("소비 아님");
            return;
        }
        Slot existingSlot = Slots.FirstOrDefault(slot => slot.CurrentItem != null && slot.CurrentItem.id == newItem.id);
        if (existingSlot !=null)
        {
            /*
             * 사용 했을 시 사용 효과 적용
             */
            existingSlot.Count--;
            existingSlot.UpdateUI();
            if (existingSlot.Count == 0)
            {
                existingSlot.ClearSlot();
            }
            Debug.Log("소비템 사용");
        }
    }


    private void OnEnable()
    {
        if (TxtGold != null)
        {
            UpdateIncome();
        }
        
    }


    public void ShowInventory()
    {
     
        //있는거 다시 집어넣기 
        for (int i = 0; i < ClientInventroy.items.Count && i < Slots.Count; i++)
        {
            
            Slots[i].SetItem(ClientInventroy.items[i]); // ✅ 아이템 UI 업데이트
            
        }
    }

    //전체 초기화 
    public void ClearInventory()
    {
        Debug.Log("🔄 인벤토리 초기화 실행");

        foreach (var slot in Slots)
        {
            slot.ClearSlot();
        }

        // 🔥 클라이언트 인벤토리의 실제 아이템 리스트 비우기
        ClientInventroy.items.Clear();
    }

    //돈 초기화
    public void UpdateIncome()
    {
        TxtGold.text = Income.ToString();
    }

    //선택한 하나 지우기
    public void UpdateInventoryUI(int soldItemId)
    {
        foreach (var slot in Slots)
        {
            if (slot.CurrentItem != null && slot.CurrentItem.id == soldItemId)
            {
                Debug.Log($"🛑 슬롯에서 {soldItemId} 제거!");
                slot.ClearSlot();
                break; // 한 개만 제거 후 종료
            }
        }
    }
    


    
}
