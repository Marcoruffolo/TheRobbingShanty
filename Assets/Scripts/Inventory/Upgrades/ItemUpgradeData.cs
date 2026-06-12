using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeCost
{
    public InventoryItemData material;
    public int amount;
}

[System.Serializable]
public class ItemUpgradeLevel
{
    public float statValue;
    public List<UpgradeCost> costs = new();
}

[CreateAssetMenu(fileName = "ItemUpgradeData", menuName = "Inventory/Upgrades/Item Upgrade")]
public class ItemUpgradeData : ScriptableObject
{
    public InventoryItemData targetItem;
    public string statName = "Daño";
    public float baseValue;
    public List<ItemUpgradeLevel> levels = new();
}
