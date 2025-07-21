using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using TMPro;
using UnityEditorInternal.Profiling.Memory.Experimental;


public class UIManager : MonoBehaviour 
{
    public GameObject invenotory;
    public GameObject Equipment;
    public GameObject BuffList;
    
    private static UIManager _instance;
    
    public List<Slot> InventorySlots = new List<Slot>();
    
    public List<EquipSlot> EquipSlots = new List<EquipSlot>();
    
    public PlayerInventory ClientInventroy;
    public TextMeshProUGUI TxtGold;
    public static UIManager Instance { get { return _instance; } }

    public Dictionary<int,Item> InventoryItems = new Dictionary<int,Item>();


    public AudioSource audioSource;
    
    public List<AudioClip> audioClips;
    
    [Header("툴팁")]
    public GameObject tooltipGroup;
    
    public TextMeshProUGUI tooltipText;
    public CanvasGroup tooltipCanvas;

    [Header("경고")]
    public GameObject warningGroup;
    public TextMeshProUGUI warningText;

    [Header("첫 초기화용")]
    public bool hasInitialized  = false;
   
    
    //플레이어가 가지고 있는 돈을 확인
    public int Income = -9999; // 서버가 초기화 해주면서 이 값은 필요 없어짐
    
    
    
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
        audioSource = GetComponent<AudioSource>();
    }
    
    
    public void EquipSetWindowActive()
    {
        if (Equipment.activeSelf == true)
        {
            Equipment.SetActive(false);
        }
        else
        {
            Equipment.SetActive(true);
        }
    }
    
    public void invenSetWindowActive()
    {
        if (invenotory.activeSelf == true)
        {
            invenotory.SetActive(false);
        }
        else
        {
            invenotory.SetActive(true);
        }
    }

    #region 사운드 실행 함수
    public void PlaySoundOpen()
    {
        foreach (var VARIABLE in audioClips)
        {
            if (VARIABLE.name ==DefineSoundName.MenuUp)
            {
                audioSource.PlayOneShot(VARIABLE);
            }
        }
    }

    public void PlaySoundClose()
    {
        
        foreach (var VARIABLE in audioClips)
        {
            if (VARIABLE.name ==DefineSoundName.MenuDown)
            {
               
                audioSource.PlayOneShot(VARIABLE);
            }
        }
    }
    
    public void PlaySoundBtnClick()
    {
        
        foreach (var VARIABLE in audioClips)
        {
            if (VARIABLE.name ==DefineSoundName.BtMouseClick)
            {
                audioSource.PlayOneShot(VARIABLE);
            }
        }
    }
    
    public void PlaySoundPickupItem()
    {
        foreach (var VARIABLE in audioClips)
        {
            if (VARIABLE.name ==DefineSoundName.PickUpItem)
            {
                audioSource.PlayOneShot(VARIABLE);
            }
        }
    }
    public void PlaySoundDlgNotice()
    {
        foreach (var VARIABLE in audioClips)
        {
            if (VARIABLE.name ==DefineSoundName.DlgNotice)
            {
                audioSource.PlayOneShot(VARIABLE);
            }
        }
    }
    
    public void PlaySoundUsePotion()
    {
        foreach (var VARIABLE in audioClips)
        {
            if (VARIABLE.name ==DefineSoundName.UsePotion)
            {
                audioSource.PlayOneShot(VARIABLE);
            }
        }
    }
    #endregion

    #region 처음 맵에 들어갈때 UI들 연동해주는 함수
    public void ConnectPlayer()
    {
        invenotory.SetActive(true);
        BuffList.SetActive(true);

        var playerObj =ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
        InventorySlots = new List<Slot>(invenotory.GetComponentsInChildren<Slot>());
        EquipSlots = new List<EquipSlot>(Equipment.GetComponentsInChildren<EquipSlot>());
        ClientInventroy = playerObj.GetComponent<YHSMyPlayerController>().playerInventory;
        invenotory.SetActive(false);
        Equipment.SetActive(false);
    }

    public void ConnectEquipment()
    {
        EquipSlots = new List<EquipSlot>(Equipment.GetComponentsInChildren<EquipSlot>());

        Equipment.SetActive(false);
    }
    #endregion

    #region 장비 장착
    public void EquipItem(Item newItem, Slot slot)
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
                    warningText.text =  $"❌ {PlayerInformation.playerStatInfo.ClassType}는 {equipment.classType} 전용 아이템을 장착할 수 없습니다!";
                    Instance.warningGroup.SetActive(true);
                    Instance.PlaySoundDlgNotice();
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
                DbChangeEquipReq(equipSlot.CurrentItem, ItemState.IsUnequipped, isFromEquipped: true); // 장비를 벗은 사실을 서버에게 알림
                SwapEquipment(slot, equipSlot);
            }
            else
            {
                UpdateItemSlot(slot);
                Debug.Log($"✅ {newItem.itemName} 장착 완료!");
            }

            // ✅ 새 장비 장착
            equipSlot.SetItem(newItem);
            DbChangeEquipReq(equipSlot.CurrentItem, ItemState.IsEquipped, isFromEquipped: false); // 장비를 장착한 사실을 서버에게 알림
        }

        // ✅ 장비 능력치 적용
        if (newItem is Equipment eq)
        {
            var playerObj =ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
            ApplyEquipmentStats(eq);
            playerObj.GetComponent<PlayerInformation>().CalculateStat();
        }
    }
    #endregion

    #region 장비창에 장착후 인벤토리에서 제거
    void UpdateItemSlot(Slot slot)
    {
        if (slot == null) return;

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
    #endregion

    #region 아이템 스왑
    void SwapEquipment(Slot inventorySlot, EquipSlot equipSlot)
    {
        if (inventorySlot == null)
        {
            return;
        }
        //수정 해야 될꺼같긴함
        Item oldItem = equipSlot.CurrentItem; // 기존 장비 저장

        // ✅ 기존 장비를 인벤토리로 이동
        if (oldItem != null)
        {
            inventorySlot.SetItem(oldItem);

            if (oldItem is Equipment equipment)
            {
                // 장착 해제에 따른 적용스탯 갱신
                PlayerInformation.equipmentStat.AttackPower -= equipment.attackPower;
                PlayerInformation.equipmentStat.MagicPower -= equipment.magicPower;
                PlayerInformation.equipmentStat.Defense -= equipment.defensePower;
            }
        }

        // ✅ 장착 슬롯 초기화
        equipSlot.CurrentItem = null;   
    }
    #endregion
    
    #region 클래스 타입에 따라 능력치 설정하는 함수
    void ApplyEquipmentStats(Equipment eq)
    {
        if (PlayerInformation.equipmentStat == null)
            PlayerInformation.equipmentStat = new PlayerStatInfo(); // null 체크 후 초기화
        

       
        var equipmentStat = PlayerInformation.equipmentStat; // 기존 객체 사용
        
   
        equipmentStat.Defense += eq.defensePower;
        switch (PlayerInformation.playerStatInfo.ClassType)
        {
            case ClassType.Archer:
                equipmentStat.AttackPower += eq.attackPower; // 궁수 보너스
                break;
            case ClassType.Magician:
                equipmentStat.MagicPower += eq.magicPower; // 마법사 보너스
                break;
            case ClassType.Warrior:
                equipmentStat.AttackPower += eq.attackPower; // 전사 보너스
                break;
        }
        Debug.Log($"⚔ 장비 장착 완료 - 공격력: {equipmentStat.AttackPower}, 방어력: {equipmentStat.Defense}, 마법력: {equipmentStat.MagicPower}");
    }
    #endregion


    #region 초기 기본값  생성
    public void InitItem()
    {
        if (hasInitialized)
        {
            return;
        }
        Equipment.SetActive(true);
        
        EquipSlot weaponItem = EquipSlots.Find(slot => slot.name == "WeaponItem");
        if (weaponItem != null)
        {
            Debug.Log("WeaponItem 발견: " + weaponItem.name);

            ClassType playerClass = PlayerInformation.playerStatInfo.ClassType; // 플레이어 직업 가져오기
            bool isUpdated = false; // 아이템이 설정되었는지 체크
            foreach (Item VARIABLE in ItemManager.Instance.ItemList)
            {
                if (VARIABLE is Equipment equipment && equipment.classType == playerClass) // 플레이어 직업과 같은 장비만 선택
                {
                    switch (playerClass)
                    {
                        case ClassType.Archer:
                            if (VARIABLE.ItemType == ItemType.Arrow1)
                            {
                                updateDefaultWeapon(weaponItem, VARIABLE);
                                Debug.Log("아쳐 장비 설정: " + VARIABLE.itemName);
                                isUpdated = true;
                            }
                            break;
                        case ClassType.Magician:
                            if (VARIABLE.ItemType == ItemType.Staff1)
                            {
                                updateDefaultWeapon(weaponItem, VARIABLE);
                                Debug.Log("매지션 장비 설정: " + VARIABLE.itemName);
                                isUpdated = true;
                            }
                            break;
                        case ClassType.Warrior:
                            if (VARIABLE.ItemType == ItemType.Sword1)
                            {
                                updateDefaultWeapon(weaponItem, VARIABLE);
                                Debug.Log("워리어 장비 설정: " + VARIABLE.itemName);
                                isUpdated = true;
                            }
                            break;
                    }

                    if (isUpdated) // 장비가 설정되었으면 루프 종료
                    {
                        var playerObj = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
                        playerObj.GetComponent<PlayerInformation>().CalculateStat();

                        break;
                    }
                }
            }

            Equipment.SetActive(false);
        }
        else
        {
            Debug.Log("WeaponItem 못찾음 ");
        }

    }

    public void InitPreItem(Inventory inventory)
    {
        // 기존의 인벤토리 슬롯을 밈
        foreach(Slot slot in InventorySlots)
        {
            slot.ClearSlot();
        }

        foreach (ItemInfo item in inventory.ItemInfo)
        {
            Item itemToAdd = ItemManager.Instance.ItemList.Find(x => x.ItemType == item.ItemType);
            if(item.Itemstate == ItemState.IsEquipped)
            {
                EquipItem(itemToAdd, null);
            }
            else
            {
                SetItem(itemToAdd, item.ItemCount);
            }
        }
    }
    #endregion

    #region UI갱신 + 정보 넣기(updateDefaultWeapon)
    //조건문의 아이템 타입과 같은 아이템 스크립트만 가져옴 
    void updateDefaultWeapon( EquipSlot weaponItem,Item VARIABLE )
    {
        //weaponItem.CurrentItem = VARIABLE;
        //weaponItem._image.sprite = VARIABLE.IconSprite;
        //Color color = weaponItem._image.color;
        //color.a = 1f;  // 
        //weaponItem._image.color = color;
                            
        //var equipmentstat = PlayerInformation.equipmentStat;
        //if (weaponItem.CurrentItem is Equipment eq)
        //{
        //    equipmentstat.AttackPower = eq.attackPower;
        //    equipmentstat.Defense = eq.defensePower;
        //    equipmentstat.MagicPower = eq.magicPower;
        //    Debug.Log("초기값 갱신");
        //    Debug.Log(equipmentstat.AttackPower);
        //    Debug.Log(equipmentstat.MagicPower);
            
        //}
    }
    #endregion

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
                    case ItemType.Hppotion1:
                    case ItemType.Hppotion2:
                        temp.SetPlayerHp(consume.healAmount);
                        //Debug.Log("체력 회복");
                        break;
                    case ItemType.Mppotion1:
                    case ItemType.Mppotion2:
                        temp.SetPlayerMp(consume.MpAmount);
                        //Debug.Log("마나 회복");
                        break;
                    case ItemType.Superpotion1:
                        temp.SetPlayerHp(PlayerInformation.playerStatInfo.MaxHp/2);
                        temp.SetPlayerMp(PlayerInformation.playerStatInfo.MaxMp/2);
                        break;
                    case ItemType.Superpotion2:
                        temp.SetPlayerHp(PlayerInformation.playerStatInfo.MaxHp);
                        temp.SetPlayerMp(PlayerInformation.playerStatInfo.MaxMp);
                        break;
                }
                PlaySoundUsePotion();
            }

            AddItem(newItem, -1, isfromEquipped: false);
            if (existingSlot.Count == 0)
            {
                existingSlot.ClearSlot();
            }
            existingSlot.UpdateUI();

            Debug.Log("소비템 사용");
        }
    }
    #endregion

    #region 아이템 추가
    // 호출 되는 경우 : 플레이어가 이 방에 입장했을 때 최초 1번 Set
    public void SetItem(Item newItem, int amount)
    {
        if (newItem == null) return;

        if (newItem.ItemType == ItemType.Gold)
        {
            HandleGold(newItem, amount: amount, isReset: true);
            return;
        }

        // 기존 슬롯에 있는 아이템인지 확인
        Slot existingSlot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem != null && slot.CurrentItem.id == newItem.id);


        if (existingSlot != null)
        {
            // 이미 존재하는 아이템이고 소비 가능한거면 재설정
            if (existingSlot.CurrentItem is Consumable consume)
            {
                UpdateConsumableSlot(existingSlot, amount, isReset: true);
            }
            // 장비는 무조건 새로 생성이지만 3번째 인자를 넣음으로써 장비아이템이 무한 증식되지 않도록
            else if (existingSlot.CurrentItem is Equipment equip)
            {
                AddToEmptySlot(newItem, amount: 1 , true, isFromEquipped: false);
            }
        }
        // 기존 슬롯에 없는 아이템이면 새로운 슬롯에 할당
        else
        {
            AddToEmptySlot(newItem, amount, true, isFromEquipped: false);
        }
    }

    // 호출 되는 경우 : 플레이어가 아이템을 획득/소비 했을 때 Add or Sub
    public void AddItem(Item newItem,int amount, bool isfromEquipped)
    {
        if (newItem == null) return;

        // 기존에 있는 아이템인지 확인
        Slot existingSlot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem != null && slot.CurrentItem.id == newItem.id);
        
        if (newItem.ItemType == ItemType.Gold)
        {
            HandleGold(newItem);
            return;  // 인벤토리에 추가되지 않도록 여기서 함수 종료
        }
        
        if (existingSlot != null)
        {
            // 🔥 이미 존재하는 아이템이고 소비 가능한거면 개수 증가 시키기
            if (existingSlot.CurrentItem is Consumable consume)
            {
                UpdateConsumableSlot(existingSlot, amount);
            }
            // 🔥 이미 존재하는 아이템이고 장비면 다른 슬롯에 넣기 
            else if (existingSlot.CurrentItem is Equipment equip)
            {
                AddToEmptySlot(newItem, amount: 1, false, isFromEquipped: isfromEquipped);
            }
        }
        else
        {
            AddToEmptySlot(newItem, amount, false, isFromEquipped : isfromEquipped);
        }
    }


    // 호출되는 경우
    // 1. 플레이어가 방에 입장할 때 (아이템 리스트 리셋 후 Add)
    // 2. 플레이어가 아이템을 먹었을 때 (기존 아이템 리스트에서 Add)
    private void HandleGold(Item newItem, int amount = 10, bool isReset = false)
    {
        if (isReset) Income = amount;
        else Income += amount;

        // 메모리 골드에도 적용
        {
            var goldItems = PlayerInformation.playerInfo.Inventory.ItemInfo
            .Where(i => i.ItemType == ItemType.Gold);


            foreach (var item in goldItems)
            {
                item.ItemCount = Income;
            }
        }

        DbChangeReq(newItem, Income);

        TxtGold.text = Income.ToString();
        Debug.Log($"💰 골드 {(isReset ? "설정" : "획득")}! 현재 보유 골드: {Income}");
    }


    // 호출되는 경우
    // 1. 플레이어가 방에 입장할 때 (아이템 리스트 리셋 후 Add)
    // 2. 플레이어가 아이템을 먹었을 때 (기존 아이템 리스트에서 Add)
    private void UpdateConsumableSlot(Slot slot, int amount, bool isReset = false)
    {
        if (isReset) slot.Count = amount;
        else slot.Count += amount;
        UpdateInventoryProto(slot.CurrentItem, amount, isReset : isReset, isEnter : false, isFromEquipped : false);
        slot.UpdateUI();

        Debug.Log($"🟢 {slot.CurrentItem.itemName} 개수 {(isReset ? "재설정" : "증가")}: {slot.Count}");
    }


    // 호출되는 경우
    // 1. 플레이어가 방에 입장할 때 (아이템 리스트 리셋 후 Add)
    // 2. 플레이어가 아이템을 먹었을 때 (기존 아이템 리스트에서 Add)
    private void AddToEmptySlot(Item newItem, int amount, bool isEnter, bool isFromEquipped)
    {
        Slot emptySlot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem == null);
        if (emptySlot != null)
        {
            emptySlot.Count += amount;
            emptySlot.SetItem(newItem);
            emptySlot.UpdateUI();
            UpdateInventoryProto(newItem, amount, isReset: true, isEnter: isEnter, isFromEquipped : isFromEquipped);
            Debug.Log($"🟢 {newItem.itemName} 개수 증가: {emptySlot.Count}");
        }
        else
        {
            Debug.LogWarning("❌ 인벤토리에 빈 슬롯이 없습니다!");
        }
    }


    // 호출되는 경우
    // 1. 플레이어가 방에 입장할 때 (아이템 리스트 리셋 후 Add)
    // 2. 플레이어가 아이템을 먹었을 때 (기존 아이템 리스트에서 Add)
    void UpdateInventoryProto(Item newItem, int amount, bool isReset = false, bool isEnter = false, bool isFromEquipped = false)
    {
        var inventoryList = PlayerInformation.playerInfo.Inventory.ItemInfo;

        // 아이템 타입별 처리
        // TODO 기준값 하드코딩 1000 제거
        if ((int)newItem.ItemType < 1000)
        {
            // 기존 동일한 아이템 있는지 확인
            ItemInfo existingProtoItem = inventoryList.FirstOrDefault(item => item.ItemId == newItem.id);

            if (existingProtoItem != null)
            {
                // 이미 있는 경우
                if (isReset) existingProtoItem.ItemCount = amount;
                else  existingProtoItem.ItemCount += amount;
                DbChangeReq(newItem, existingProtoItem.ItemCount);

                Debug.Log($"🧪 프로토 인벤토리 소비아이템 수량 증가: {newItem.itemName}, 개수: {existingProtoItem.ItemCount}");
            }
            else
            {
                // 없으면 새로 생성
                ItemInfo newProtoItem = CreateProtoItem(newItem, amount);
                inventoryList.Add(newProtoItem);
                DbChangeReq(newItem, newProtoItem.ItemCount);
                Debug.Log($"🧪 프로토 인벤토리 소비아이템 새로 생성: {newItem.itemName}, 개수: {amount}");
            }
        }
        // TODO 기준값 하드코딩 1000 제거
        else if ((int)newItem.ItemType > 1000)
        {
            // isEnter(각 맵에 입장) 시엔 서버 DB 아이템을 추가해선 안됨, 
            if (isEnter == true) return;
            // 장비는 무조건 새로 생성
            var newProtoItem = CreateProtoItem(newItem, amount);
            inventoryList.Add(newProtoItem);
            DbChangeEquipReq(newItem, ItemState.IsUnequipped, isFromEquipped : isFromEquipped);
            Debug.Log($"🗡️ 프로토 인벤토리 장비 추가: {newItem.itemName}");
        }
        else
        {
            Debug.LogWarning($"⚠️ 알 수 없는 아이템 타입: {newItem.ItemType}");
        }

    }

    void DbChangeReq(Item newItem, int amount)
    {
        C_Iteminfo itemPkt = new C_Iteminfo();
        itemPkt.ItemInfo = new ItemInfo();
        itemPkt.ItemInfo.ItemType = newItem.ItemType;
        itemPkt.ItemInfo.ItemCount = amount;
        NetworkManager.Instance.Send(itemPkt);
    }

    // 장비 장착/해제용 함수
    public void DbChangeEquipReq(Item newItem, ItemState isEquip, bool isFromEquipped = true)
    {
        if (newItem.category != Item.ItemCategory.Equipment)
        {
            Debug.Log(" 장비가 아님 ");
            return;
        }
        C_Iteminfo itemPkt = new C_Iteminfo();
        itemPkt.ItemInfo = new ItemInfo();
        itemPkt.ItemInfo.ItemType = newItem.ItemType;
        itemPkt.ItemInfo.Itemstate = isEquip;
        itemPkt.ItemInfo.ItemCount = 1; // ITEM은 무조건 1개씩 다룸
        itemPkt.ItemInfo.IsFromEquipped = isFromEquipped;
        NetworkManager.Instance.Send(itemPkt);
    }

    ItemInfo CreateProtoItem(Item item, int amount)
    {
        return new ItemInfo
        {
            ItemId = item.id,
            ItemCategory = 0, // TODO
            ItemType = item.ItemType,
            Itemstate = 0, // TODO
            ItemCount = amount,
            OwnerId = PlayerInformation.playerInfo.PlayerId, // NOT USED
            CanRootAnyOne = false, // NOT USED
            PositionX = 0, // NOT USED
            PositionY = 0 // NOT USED
        };
    }

    #endregion

    #region 타입별 정렬

    public void TypeCheck()
    {
        foreach (var inven in InventorySlots)
        {
            if (inven.CurrentItem is Equipment eq)
            {
                Debug.Log(eq.itemName);
            }
        }
    }

    #endregion

}
