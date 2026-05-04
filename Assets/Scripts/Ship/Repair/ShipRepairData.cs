using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RepairRequirement
{
    public InventoryItemData itemData;
    public int requiredAmount;
    [System.NonSerialized] public int Deposited;
}

[CreateAssetMenu(fileName = "ShipRepairData", menuName = "Ship/Repair Data")]
public class ShipRepairData : ScriptableObject
{
    public List<RepairRequirement> requirements = new List<RepairRequirement>();
}
