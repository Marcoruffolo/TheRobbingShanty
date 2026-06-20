using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Inventory/Weapon Item")]
public class WeaponItemData : InventoryItemData
{
    public WeaponStats stats = new WeaponStats();
    public ItemUpgradeData damageUpgrade;
}
