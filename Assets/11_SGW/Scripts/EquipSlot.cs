using System;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;

public class EquipSlot : MonoBehaviour
{

    public Image _image;
    public Item CurrentItem;

    public Equipment.Parts CurrentPart;
    
    private void Awake()
    {
        _image = GetComponent<Image>();
    }

    public void SetItem(Item item)
    {
        CurrentItem = item;

        _image.sprite = item.IconSprite;

        Color color = _image.color;
        color.a = 1f; // ✅ 아이콘을 완전히 불투명하게 설정
        _image.color = color;
    }

    public void ClearSlot()
    {
        Color color = _image.color;
        //if (Count < 1)
        {
            color.a = 0f; // ✅ 아이콘을 완전히 투명하게 설정
            _image.color = color;
            CurrentItem = null; // 아이템 제거
        }
    }
}
