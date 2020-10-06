using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/ItemData", order = 1)]
public class ItemSO : ScriptableObject
{
    public string itemName;
    public int itemID;
    public Sprite icon;
    public int itemCount;
    private int chestSlotID = -1;

    public enum ItemType { Tool, Weapon, RawResource, Manufactured , Other};
    public ItemType itemType;
    public void SetChestSlotID(int newID)
    {
        chestSlotID = newID;
    }
    public int GetChestSlotID() { return chestSlotID; }
}
