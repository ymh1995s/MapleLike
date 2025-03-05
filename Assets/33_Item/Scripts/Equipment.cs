using UnityEngine;


[CreateAssetMenu(fileName = "Equipment ", menuName = "Scriptable Objects/Equipment ")]
public class Equipment  : Item
{
    public enum Job
    {
        None,
        Warrier,
        Wizard,
        Ranger
    }

    public int limitLevel;
    public Job limitJob;
    //보너스 스텟
    public int attackPower; // 공격력 증가
    public int defensePower; // 방어력 증가
    
    
    public Equipment()
    {
        category = ItemCategory.Equipment; // 장비로 설정
    }
}
