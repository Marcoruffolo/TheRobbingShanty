using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipUpgradeRegistry", menuName = "Ship/Upgrades/Ship Upgrade Registry")]
public class ShipUpgradeRegistry : ScriptableObject
{
    public List<ShipUpgradeData> upgrades = new();
}
