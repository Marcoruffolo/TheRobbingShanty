using System.Collections.Generic;
using UnityEngine;

public enum ShipStat
{
    MaxDurability,
    Speed
}

[CreateAssetMenu(fileName = "ShipUpgradeData", menuName = "Ship/Upgrades/Ship Upgrade")]
public class ShipUpgradeData : ScriptableObject
{
    public ShipStat stat;
    public Sprite icon;
    public string statName = "Durabilidad";
    public float baseValue;
    public List<ItemUpgradeLevel> levels = new();
}
