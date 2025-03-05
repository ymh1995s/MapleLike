using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    //아이템 아이템 ID(Json기반) , 아이템 갯수 
    Dictionary<int, int> inventory;
    
    public List<Item> items = new List<Item>();
    
    //플레이어가 가지고 있는 돈을 확인
    public int Income;

    
    private void Start()
    {
        inventory = new Dictionary<int, int>();
    }

    public void AddItem(int itemId)
    {
        if (inventory.ContainsKey(itemId))
        {
            inventory[itemId]++;
        }
        else
        {
            inventory.Add(itemId, 0);
        }
    }

    public void RemoveItem(int itemId)
    {
        if (inventory.ContainsKey(itemId))
        {
            inventory[itemId]--;
        }
        else
        {
            inventory.Remove(itemId);
        }
    }
}
