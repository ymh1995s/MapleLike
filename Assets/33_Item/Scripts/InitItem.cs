using System;
using Google.Protobuf.Protocol;
using UnityEngine;


public class InitItem : MonoBehaviour
{
    [SerializeField] ItemType itemType;
    [SerializeField] string itemName;
    public Item Property;
    
    
    void Start()
    {
        itemName = gameObject.name;

        if (Enum.TryParse(itemName, out ItemType parsedType))
        {
            itemType = parsedType;
        }
        else
        {
            Debug.LogWarning($"'{itemName}'은(는) ItemType에 존재하지 않습니다.");
        }

        foreach (var item in ItemManager.Instance.ItemList)
        {
            if (itemType == item.ItemType)
            {
                Property = item;
                break;
                
            }
        }
        if (Property == null)
        {
            Debug.LogWarning($"'{itemType}'에 해당하는 아이템을 찾지 못했습니다.");
        }
    }

  
}
