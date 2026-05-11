using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootEntry
{
    public InventoryItemData item;
    [Range(0f, 1f)] public float probability = 0.5f;
    [Min(1)] public int minAmount = 1;
    [Min(1)] public int maxAmount = 5;
}

[CreateAssetMenu(fileName = "LootTable", menuName = "Inventory/Loot Table")]
public class ChestLootTable : ScriptableObject
{
    public List<LootEntry> entries = new List<LootEntry>();
}
