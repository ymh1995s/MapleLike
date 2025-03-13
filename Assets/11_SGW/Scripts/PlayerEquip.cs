using System;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;

public class PlayerEquip : MonoBehaviour
{
    //자식오브젝트에 들어있는 Slot 스크립트 전부 가져오기 
    public List<EquipSlot> Slots = new List<EquipSlot>();
    public PlayerEquip playerEquip;

    public GameObject Player;

    void Start()
    {
        Slots = new List<EquipSlot>(transform.GetComponentsInChildren<EquipSlot>());
        Player = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
        //게임 오브젝트 가져오기
        playerEquip = Player.GetComponent<YHSMyPlayerController>().playerEquip;
        Player.GetComponent<YHSMyPlayerController>().Equipment.gameObject.SetActive(false);
    }



    public int EquipItem(Item newItem,Slot slot)
    {
        // 누른 아이템이 장비라면
        if (newItem.category == Item.ItemCategory.Equipment)
        {
            foreach (var equipSlot in Slots)
            {
                if (equipSlot.CurrentItemType == newItem.ItemType)
                {
                    // 사용했을 시 사용 효과 적용
                    equipSlot.SetItem(newItem);
                    
                    //인벤토리 지우기
                    slot._image.sprite = null;
                    Color color = slot._image.color;
                    color.a = 1f; // 알파 값 1 (완전 불투명)
                    slot._image.color = color;
                    slot.Count = 0;
                    slot.CurrentItem = null;
                    //장비 스크립터블 오브젝트 가져오기
                    if (newItem is Equipment eq)
                    {
                        int statValue = newItem.ItemType switch
                        {
                            ItemType.Armor or ItemType.Helmet or ItemType.Boots => eq.defensePower,
                            ItemType.Arrow or ItemType.Sword => eq.attackPower,
                            ItemType.Staff  => eq.magicPower,
                            _ => 0 // 기본값 (예: 방어력/공격력이 없는 경우)
                        };
                        Debug.Log(statValue);
                        return statValue;
                    }

                    return 0; // Equipment가 아닐 경우 기본값 반환
                }
            }
        }

        return 0; // 장비가 아니거나, 조건을 만족하는 장비 슬롯이 없을 경우 기본값 반환
    }

    





}
