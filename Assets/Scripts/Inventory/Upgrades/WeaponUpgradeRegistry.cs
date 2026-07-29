using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponUpgradeRegistry", menuName = "Inventory/Upgrades/Weapon Upgrade Registry")]
public class WeaponUpgradeRegistry : ScriptableObject
{
    public List<ItemUpgradeData> upgrades = new();
}
