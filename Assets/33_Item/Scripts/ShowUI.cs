using System;
using System.Linq;
using Google.Protobuf.Protocol;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShowUI : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    public Info UIPrefab;
    public GameObject TooltipGroup;
    
    public PlayerInventory ClientInventroy;
    public TextMeshProUGUI TxtCurrentMeso;
    public TextMeshProUGUI Tooltip;
    //쓸데없는 곳 Raycast를 막기 위한 함수,툴팁을 위한것
    public CanvasGroup tooltipCanvas;
    public Info.OwnerType ownerType;
    
    void Start()
    {
        
        // 마우스 이벤트  여러번 방지
        if (Tooltip != null)
        {
            
            tooltipCanvas = TooltipGroup.GetComponent<CanvasGroup>();
            if (tooltipCanvas != null)
            {
                tooltipCanvas.blocksRaycasts = false; 
            }
        }
    }

  
    
    public void InitUI()
    {
        // 아이템 로딩 완료 이벤트를 구독
        if (ItemManager.Instance != null)
        {
            // 수정 : 구독이 아닌 함수실행으로 
            // ItemManager.Instance.OnItemsLoaded += UpdateUI;
            
            UpdateUI();
            
        }
        else
        {
            Debug.LogWarning("ItemManager.Instance가 null입니다.");
        }
    }



    private void OnEnable()
    {
        InitUI();
        ItemManager.Instance.OnItemsLoaded += UpdateUI;
    }
    private void OnDisable()
    {
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.OnItemsLoaded -= UpdateUI;
        }
    }
    

    #region 최초 상점 UI 갱신
    public void UpdateUI()
    {
        // 기존 UI를 모두 삭제하고 새로 생성
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject); // 기존 아이템 UI 삭제
        }
        if (ownerType == Info.OwnerType.Shop)
        {
          
            foreach (var itemData in ItemManager.Instance.ItemList)
            {
                if (itemData.ItemType ==ItemType.Gold ) // 예: 소비 아이템 제외
                    continue;
                var newItemUI = Instantiate(UIPrefab, transform); 
                newItemUI.SetInfo(itemData, ownerType);
            }
        }
       
        if (ownerType == Info.OwnerType.Player)
        {
            ClientInventroy = ObjectManager.Instance.myplayerTest.GetComponent<YHSMyPlayerController>().playerInventory;
            //아이디 이름을 어떻게 찾아오나 ?
            var Testobj = ObjectManager.Instance.FindById(0);
         
            if (TxtCurrentMeso != null)
            {
                 TxtCurrentMeso.text = ClientInventroy.Income.ToString();
            }
            foreach (var itemData in ClientInventroy.items)
            { 
                var newItemUI = Instantiate(UIPrefab, transform); 
                newItemUI.SetInfo(itemData, ownerType);
            }
          
        }
     
    }
    #endregion

    #region 아이템 구매 함수
    /// <summary>
    /// 아이템 구매 및 UI갱신 같이
    /// </summary>
    public void BuyItem()
    {
        
        ClientInventroy = ObjectManager.Instance.myplayerTest.GetComponent<YHSMyPlayerController>().playerInventory;
        
        var a = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
        Debug.Log("Player"+ a);
        
        if (ClientInventroy == null)
        {
            Debug.LogError("❌ ClientInventroy가 설정되지 않았습니다!");
            return;
        }
        //아이템 -1 번지로 초기화 기본값으로 --> 아무것도 없는 상태를 말함
        int itemId = -1;

        
        //클릭된 오브젝트의 아이템 아이디 값을 가져오는 구문
        foreach (Transform childTransform in transform)
        {
            var component = childTransform.GetComponent<ChangeImageOnClick>();
            if (component != null && component.CurrentClickedGameObject != null)
            {
                itemId = component.CurrentClickedGameObject.GetComponent<Info>().ID;
                break;
            }
        }
        
        if (itemId == -1)
        {
            return;
        }
        /*
         * 1.아이템아이디를 아이템리스트에서 맞는걸 찾아서 할당
         * 2.플레이어의 메소와 비교해서 구매 가능 여부를 확인
         * P.S 플레이어 스탯 및 플레이어의 메소 파라미터가  연결이 안되 테스트용 PlayerTestIncome을 만들어서 사용
         */
        
        Item itemToAdd = ItemManager.Instance.ItemList.Find(item => item.id == itemId);
        if (itemToAdd != null)
        {
            if (ClientInventroy.Income < itemToAdd.buyprice)
            {
                //아마 팝업 UI를 뜨게 할예정
                Debug.Log("돈 없다. 돈모아와라");
                return;
            }
            ClientInventroy.Income -= itemToAdd.buyprice;
            ClientInventroy.items.Add(itemToAdd);
        }
        else
        {
            Debug.LogWarning("⚠ 아이템을 찾을 수 없습니다.");
            return;
        }

        // 🔥 OnItemsLoaded 이벤트 강제 트리거
        Debug.Log("OnItemsLoaded 강제 트리거");
        ItemManager.Instance.TriggerOnItemsLoaded();
        
    }
    #endregion

    #region 아이템 판매 함수
    /// <summary>
    /// 아이템 판매 및 UI갱신 같이
    /// </summary>
    /*
     * 구조는 구매와 동일 대신 빼기만 사용
     */
    public void SellItem()
    {
        if (ClientInventroy == null)
        {
            Debug.LogError("❌ ClientInventroy가 설정되지 않았습니다!");
            return;
        }

        int itemId = -1;

        foreach (Transform childTransform in transform)
        {
            var component = childTransform.GetComponent<ChangeImageOnClick>();
            if (component != null && component.CurrentClickedGameObject != null)
            {
                itemId = component.CurrentClickedGameObject.GetComponent<Info>().ID;
                Debug.Log($"🎯 선택된 아이템 ID: {itemId}");
                break;
            }
        }

        if (itemId == -1)
        {
            Debug.LogWarning("⚠ 아이템이 선택되지 않았거나 ID를 찾을 수 없습니다.");
            return;
        }

        var itemToAdd = ItemManager.Instance.ItemList.Find(item => item.id == itemId);
        if (itemToAdd != null)
        {
            ClientInventroy.items.Remove(itemToAdd);
            ClientInventroy.Income += itemToAdd.sellprice;
            Debug.Log("판매 완료");
        }
        else
        {
            Debug.LogWarning("⚠ 아이템을 찾을 수 없습니다.");
            return;
        }

        // 🔥 OnItemsLoaded 이벤트 강제 트리거
        Debug.Log("OnItemsLoaded 강제 트리거");
        ItemManager.Instance.TriggerOnItemsLoaded();
        
    }
    #endregion

    
    //마우스가 가르킨 오브젝트의 정보에 맞게 툴팁 설명 해주기
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject hoveredObject = eventData.pointerEnter;
        Info hoveredInfo;
        if (hoveredObject.TryGetComponent<Info>(out hoveredInfo))
        {
            foreach (var itemData in ItemManager.Instance.ItemList)
            {
                if (itemData.id == hoveredInfo.ID)
                {
                    if (Tooltip != null)
                    {
                        TooltipGroup.SetActive(true);
                        Tooltip.text = itemData.description;
                        TooltipGroup.transform.position = eventData.position;
                    }
                }
            }
        }
        else
        {
            // 부모 오브젝트가 없거나, Info 컴포넌트가 없음
            Debug.Log("부모 오브젝트가 없거나 Info 컴포넌트가 없음");
        }
    }
    //마우스가 가르킨 오브젝트 없으면끄기
    public void OnPointerExit(PointerEventData eventData)
    {
        if (Tooltip != null)
        {
            TooltipGroup.SetActive(false);
        }
    }
}