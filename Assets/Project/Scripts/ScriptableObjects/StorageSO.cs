using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/ChestData", order = 2)]
public class StorageSO : ScriptableObject
{
    public string chestName;
    public int chestTotalSlots;
    public List<ItemSO> contentsInChest = new List<ItemSO>();
}
