using UnityEngine;

public class PlayerEquip : MonoBehaviour
{
   

    void Start()
    {
        // Player.GetComponent<YHSMyPlayerController>().Equipment.gameObject.SetActive(false);
        UIManager.Instance.ConnectEquipment();
        UIManager.Instance.InitItem();
    }





    





}
