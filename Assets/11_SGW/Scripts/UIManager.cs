using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public GameObject invenotory;
    public GameObject Equipment;
    
    private static UIManager _instance;
    
    public List<Slot> InventorySlots = new List<Slot>();
    
    public List<EquipSlot> EquipSlots = new List<EquipSlot>();
    
    public PlayerInventory ClientInventroy;
    public TextMeshProUGUI TxtGold;
    public static UIManager Instance { get { return _instance; } }
    
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    

    public void ConnectPlayer()
    {
        invenotory.SetActive(true);
        
        var playerObj =ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
        InventorySlots = new List<Slot>(invenotory.GetComponentsInChildren<Slot>());
        ClientInventroy = playerObj.GetComponent<YHSMyPlayerController>().playerInventory;
        TextMeshProUGUI[] textComponents = GetComponentsInChildren<TextMeshProUGUI>();
        // foreach (var text in textComponents)
        // {
        //     if (text.gameObject.name == "TxtGold")
        //     {
        //         TxtGold = text; // 정확한 TxtGold 찾기
        //         Debug.Log("🎯 정확한 TxtGold 찾음: " + TxtGold.text);
        //         break; // 찾았으면 더 이상 반복할 필요 없음
        //     }
        // }
        invenotory.SetActive(false);
    }

    public void ConnectEquipment()
    {
        EquipSlots = new List<EquipSlot>(Equipment.GetComponentsInChildren<EquipSlot>());
        
        Equipment.SetActive(false);
    }
    
    public void EquipItem2(Item newItem, Slot slot)
    {
        // ✅ 장비인지 확인
        if (newItem.category != Item.ItemCategory.Equipment)
        {
            Debug.Log("⚠ 장비 아이템이 아닙니다!");
            return;
        }

        if (newItem is Equipment equipment)
        {
            // ✅ Cnone 타입이면 모든 직업이 장착 가능
             if (equipment.classType == ClassType.Cnone)
            {
                Debug.Log($"🛡 {newItem.itemName}은(는) 모든 직업이 장착할 수 있습니다!");
            }
            else
            {
                // ✅ 직업별 무기 제한 적용
                if (equipment.classType != PlayerInformation.playerStatInfo.ClassType)
                {
                    Debug.Log($"❌ {PlayerInformation.playerStatInfo.ClassType}는 {equipment.classType} 전용 아이템을 장착할 수 없습니다!");
                    return;
                }
            }
        }
        
        // ✅ 부위에 맞는 장비 슬롯 찾기
        if (newItem is Equipment eq1)
        {
            EquipSlot equipSlot = EquipSlots.FirstOrDefault(slot => slot.CurrentPart == eq1.parts);
        
            if (equipSlot == null)
            {
                Debug.Log("⚠ 해당 장비를 장착할 슬롯이 없습니다.");
                return;
            }

            // ✅ 기존 장착된 아이템 확인 후 인벤토리로 이동
            if (equipSlot.CurrentItem != null)
            {
                Debug.Log($"🔄 {equipSlot.CurrentItem.itemName} → {newItem.itemName} 장비 교체!");
                SwapEquipment(slot, equipSlot);
            }
            else
            {
                UpdateItemSlot(slot);
                Debug.Log($"✅ {newItem.itemName} 장착 완료!");
            }

            // ✅ 새 장비 장착
            equipSlot.SetItem(newItem);
        }

        // ✅ 장비 능력치 적용
        if (newItem is Equipment eq)
        {
            ApplyEquipmentStats(eq);
        }
    }
    
    void UpdateItemSlot(Slot slot)
    {
        if (slot.Count > 1)
        {
            slot.Count--;
            slot.CountText.text = slot.Count.ToString();
        }
        else
        {
            // 인벤토리에서 제거
            slot._image.sprite = null;
            Color color = slot._image.color;
            color.a = 0f; // 투명 처리
            slot._image.color = color;
            slot.Count = 0;
            slot.CurrentItem = null;
            Debug.Log("🗑 아이템이 하나뿐이어서 삭제됨");
        }
    }
    void SwapEquipment(Slot inventorySlot, EquipSlot equipSlot)
    {
        Item oldItem = equipSlot.CurrentItem; // 기존 장비 저장

        // ✅ 기존 장비를 인벤토리로 이동
        if (oldItem != null)
        {
            inventorySlot.SetItem(oldItem);
        }

        // ✅ 장착 슬롯 초기화
        equipSlot.CurrentItem = null;   
    }

    // ✅ 클래스 타입에 따라 능력치 설정하는 함수
    void ApplyEquipmentStats(Equipment eq)
    {
        if (PlayerInformation.equipmentStat == null)
            PlayerInformation.equipmentStat = new PlayerStatInfo(); // null 체크 후 초기화

        var equipmentStat = PlayerInformation.equipmentStat; // 기존 객체 사용
        equipmentStat.Defense = eq.defensePower;
        switch (PlayerInformation.playerStatInfo.ClassType)
        {
            case ClassType.Archer:
                equipmentStat.AttackPower = eq.attackPower; // 궁수 보너스
                break;
            case ClassType.Magician:
                equipmentStat.MagicPower = eq.magicPower; // 마법사 보너스
                break;
            case ClassType.Warrior:
                equipmentStat.AttackPower = eq.attackPower; // 전사 보너스
                break;
        }

        Debug.Log($"⚔ 장비 장착 완료 - 공격력: {equipmentStat.AttackPower}, 방어력: {equipmentStat.Defense}, 마법력: {equipmentStat.MagicPower}");
    }
    
    

    public void InitItem()
    {
        Equipment.SetActive(true);
        
        EquipSlot weaponItem = EquipSlots.Find(slot => slot.name == "WeaponItem");
        if (weaponItem != null)
        {
            Debug.Log("WeaponItem 발견: " + weaponItem.name);
            foreach (Item VARIABLE in ItemManager.Instance.ItemList)
            {
                //(경원)임시 현승님 오시면 수정 사항 
                //수정을 어떻게 해야되나 직업 클래스 타입으로 받아서 넣어야 한다.
                //현재는  WeaponItem의 무기 타입을 보고 넣고있다.
                //이렇게 넣으면 무기가 많아지면 무기타입만 보고 넣기에는  오류가 날 것으로 예상 
                if (VARIABLE is Equipment equipment)
                {
                    switch (equipment.classType)
                    {
                        case ClassType.Archer:
                            if (VARIABLE.ItemType == ItemType.DefaultArrow)
                            {
                                updateDefaultWeapon(weaponItem, VARIABLE);
                            }
                            Debug.Log("아쳐");
                            break;
                        case ClassType.Magician:
                            if (VARIABLE.ItemType == ItemType.DefaultStaff)
                            {
                                updateDefaultWeapon(weaponItem, VARIABLE);
                            }
                            Debug.Log("매지션");
                            break;
                        case ClassType.Warrior:
                            if (VARIABLE.ItemType == ItemType.DefaultSword)
                            {
                                updateDefaultWeapon(weaponItem, VARIABLE);
                            }
                            Debug.Log("워리어");
                            break;
                        case ClassType.Cnone:
                            updateDefaultWeapon(weaponItem, VARIABLE);
                            break;
                    }
                    Equipment.SetActive(false);
                }
            }
        }
        else
        {
            Debug.Log("WeaponItem 못찾음 ");
        }
    }

    void updateDefaultWeapon( EquipSlot weaponItem,Item VARIABLE )
    {
        weaponItem.CurrentItem = VARIABLE;
        weaponItem._image.sprite = VARIABLE.IconSprite;
        Color color = weaponItem._image.color;
        color.a = 1f;  // 
        weaponItem._image.color = color;
                            
        var equipmentstat = PlayerInformation.equipmentStat;
        if (weaponItem.CurrentItem is Equipment eq)
        {
            equipmentstat.AttackPower = eq.attackPower;
            equipmentstat.Defense = eq.defensePower;
            equipmentstat.MagicPower = eq.magicPower;
            Debug.Log("초기값 갱신");
            Debug.Log(equipmentstat.AttackPower);
        }
    }

    #region 아이템 사용

    public void UseItem(Item newItem)
    {
        if (newItem == null) { return; }

        if (newItem.category == Item.ItemCategory.Equipment)
        {
            Debug.Log("소비 아님");
            return;
        }
        Slot existingSlot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem != null && slot.CurrentItem.id == newItem.id);
        if (existingSlot !=null)
        {
            /*
             * 사용 했을 시 사용 효과 적용
             */
            PlayerInformation temp = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id).GetComponent<PlayerInformation>();

            if (existingSlot.CurrentItem is Consumable consume)
            {
                switch (existingSlot.CurrentItem.ItemType)
                {
                    case ItemType.Hppotion:
                        temp.SetPlayerHp(consume.healAmount);
                        Debug.Log("체력 회복");
                        break;
                    case ItemType.Mppotion:
                        temp.SetPlayerMp(consume.MpAmount);
                        Debug.Log("마나 회복");
                        break;
                }
            }


            existingSlot.Count--;
            existingSlot.UpdateUI();
            if (existingSlot.Count == 0)
            {
                existingSlot.ClearSlot();
            }
            Debug.Log("소비템 사용");
        }
    }

    #endregion
    public void AddItem(Item newItem)
    {
        if (newItem == null)
        {
            return;
        }
        // 기존에 있는 아이템인지 확인
        Slot existingSlot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem != null && slot.CurrentItem.id == newItem.id);
        
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
            Slot emptySlot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem == null);
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


}
