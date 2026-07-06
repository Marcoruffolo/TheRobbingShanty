using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public class InventorySystem
{
    [SerializeField] private List<InventorySlot> inventorySlots;

    public List<InventorySlot> InventorySlots => inventorySlots;
    public int InventorySize => inventorySlots.Count;

    public UnityAction<InventorySlot> OnInventorySlotChanged;

    public InventorySystem(int size)
    {
        inventorySlots = new List<InventorySlot>(size);

        for (int i = 0; i < size; i++)
        {
            inventorySlots.Add(new InventorySlot());
        }
    }

    public bool AddToInventory(InventoryItemData itemToAdd, int amountToAdd)
    {
        if (ContainsItem(itemToAdd, out List<InventorySlot> invSlots))
        {
            foreach (var slot in invSlots)
            {
                if (slot.EnoughRoomLeftInStack(amountToAdd))
                {
                    slot.AddToStack(amountToAdd);
                    OnInventorySlotChanged?.Invoke(slot);
                    return true;
                }
            }
        }
        

        if (HasFreeSlot(out InventorySlot freeSlot))
        {
            if (freeSlot.EnoughRoomLeftInStack(amountToAdd))
            {
                freeSlot.UpdateInventorySlot(itemToAdd, amountToAdd);
                OnInventorySlotChanged?.Invoke(freeSlot);
                return true;
            }
        }

        return false;
    }

    public bool ContainsItem(InventoryItemData itemToAdd, out List<InventorySlot> invSlot)
    {
        invSlot = inventorySlots.Where(i => i.ItemData == itemToAdd).ToList();
        return invSlot == null ? false : true;
    }

    public bool HasFreeSlot(out InventorySlot freeSlot)
    {
        freeSlot = InventorySlots.FirstOrDefault(i => i.ItemData == null);
        return freeSlot == null ? false : true;
    }

    public bool IsEmpty() => inventorySlots.All(s => s.ItemData == null);

    public int GetItemCount(InventoryItemData item)
    {
        return inventorySlots.Where(s => s.ItemData == item).Sum(s => s.StackSize);
    }

    public bool UseItem(InventorySlot slot)
    {
        if(slot.ItemData != null) 
        {
            if(slot.ItemData.itemtype == ItemType.None || slot.ItemData.itemtype == ItemType.Arma) return false;
            Debug.Log("Use item");
            slot.ItemData.UseItem();
            RemoveItem(slot.ItemData,1);
            return true;
        }

        return false;
    }

    public bool RemoveItem(InventoryItemData item, int amount)
    {
        if (GetItemCount(item) < amount) return false;

        foreach (var slot in inventorySlots.Where(s => s.ItemData == item))
        {
            if (amount <= 0) break;

            int toRemove = Mathf.Min(slot.StackSize, amount);
            slot.RemoveFromStack(toRemove);
            amount -= toRemove;

            if (slot.StackSize <= 0) slot.ClearSlot();

            OnInventorySlotChanged?.Invoke(slot);
        }

        return true;
    }
}