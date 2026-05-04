using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SOInventorySaveData", menuName = "Inventory/Save Data")]
public class SOInventorySaveData : ScriptableObject
{
    [System.Serializable]
    public class SlotData
    {
        public InventoryItemData itemData;
        public int stackSize;
    }

    public bool hasSavedData;
    public List<SlotData> primarySlots = new();
    public List<SlotData> secondarySlots = new();

    public void Save(InventorySystem primary, InventorySystem secondary)
    {
        primarySlots.Clear();
        secondarySlots.Clear();

        foreach (var slot in primary.InventorySlots)
            primarySlots.Add(new SlotData { itemData = slot.ItemData, stackSize = slot.StackSize });

        foreach (var slot in secondary.InventorySlots)
            secondarySlots.Add(new SlotData { itemData = slot.ItemData, stackSize = slot.StackSize });

        hasSavedData = true;
    }

    public void Restore(InventorySystem primary, InventorySystem secondary)
    {
        if (!hasSavedData) return;

        for (int i = 0; i < primarySlots.Count && i < primary.InventorySlots.Count; i++)
        {
            if (primarySlots[i].itemData != null)
                primary.InventorySlots[i].UpdateInventorySlot(primarySlots[i].itemData, primarySlots[i].stackSize);
        }

        for (int i = 0; i < secondarySlots.Count && i < secondary.InventorySlots.Count; i++)
        {
            if (secondarySlots[i].itemData != null)
                secondary.InventorySlots[i].UpdateInventorySlot(secondarySlots[i].itemData, secondarySlots[i].stackSize);
        }
    }

    public void Clear()
    {
        primarySlots.Clear();
        secondarySlots.Clear();
        hasSavedData = false;
    }
}
