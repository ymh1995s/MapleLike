using Google.Protobuf.Protocol;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerInformation playerInfo;   // 테스트용

    private GameObject player;
    public float pickupRange = 0.7f;  // 줍기 범위
    public LayerMask itemLayer;   
    
    private void Start()
    {
        playerInfo = GetComponent<PlayerInformation>();
        player =  ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
        itemLayer = 1 << LayerMask.NameToLayer("Item");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            //playerInfo.PrintStatInfo();
            UIManager.Instance.PlaySoundOpen();
            StatWindowManager.Instance.SetWindowActive();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            
            if (!UIManager.Instance.invenotory.activeSelf)
            {
                UIManager.Instance.PlaySoundOpen();
            }
            UIManager.Instance.invenSetWindowActive();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!UIManager.Instance.Equipment.activeSelf)
            {
                UIManager.Instance.PlaySoundOpen();
            }
            UIManager.Instance.EquipSetWindowActive();
            
        }       
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("GetItem");
            ObjectManager.Instance.PickupNearbyItems2();
        }
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            var Obj = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
            Debug.Log(Obj.GetComponent<PlayerController>().isDead);
            if (Obj.GetComponent<PlayerController>().isDead)
            {
                return;
            }
            foreach (var VARIABLE in UIManager.Instance.InventorySlots)
            {
                //없을것을 대비한 널 체크 추가 
                if (VARIABLE.CurrentItem != null && VARIABLE.CurrentItem.ItemType == ItemType.Hppotion)
                {
                    UIManager.Instance.UseItem(VARIABLE.CurrentItem);
                    break;
                }
            }
            Debug.Log("UseItem HP");
        }
        
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            var Obj = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
            Debug.Log(Obj.GetComponent<PlayerController>().isDead);
            if (Obj.GetComponent<PlayerController>().isDead)
            {
                return;
            }
            
            foreach (var VARIABLE in UIManager.Instance.InventorySlots)
            {
                //없을것을 대비한 널 체크 추가
                if (VARIABLE.CurrentItem != null && VARIABLE.CurrentItem.ItemType == ItemType.Mppotion)
                {
                    UIManager.Instance.UseItem(VARIABLE.CurrentItem);
                    break;
                }
            }
            Debug.Log("UseItem MP");
        }

        #region 테스트를 위한 임시 단축키
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // 체력 깎기
            playerInfo.SetPlayerHp(-10);
        }
        if (Input.GetKeyDown(KeyCode.U))
        {
            playerInfo.SetPlayerExp(12);
        }
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.U))
        {
            playerInfo.SetPlayerExp(PlayerInformation.playerStatInfo.TotalExp);
        }
        #endregion
    }
}
