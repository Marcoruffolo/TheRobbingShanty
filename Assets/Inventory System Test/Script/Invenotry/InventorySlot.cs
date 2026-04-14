using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

[System.Serializable]
public class InventorySlot
{
    [SerializeField] private InventoryItemData itemData;
    [SerializeField] private int stackSize;

    public InventoryItemData ItemData => itemData;
    public int StackSize => stackSize;

    public InventorySlot(InventoryItemData source, int amount)
    {
        itemData = source;
        stackSize = amount;
    }

    public InventorySlot()
    {
        ClearSlot();
    }

    public void ClearSlot()
    {
        itemData = null;
        stackSize = -1;
    }

    public void AssignItem(InventorySlot invSlot)
    {
        if (itemData == invSlot.ItemData)
        {
            AddToStack(invSlot.stackSize);
        }
        else
        {
            itemData = invSlot.itemData;
            stackSize = 0;
            AddToStack(invSlot.stackSize);
        }
    }

    public void UpdateInventorySlot(InventoryItemData data, int amount)
    {
        itemData = data;
        stackSize = amount;
    }

    public bool EnoughRoomLeftInStack(int amountToAdd, out int amountremaining)
    {
        amountremaining = ItemData.maxStackSize - stackSize;

        return EnoughRoomLeftInStack(amountToAdd);
    }

    public bool EnoughRoomLeftInStack(int amountToAdd)
    {
        if(itemData == null || itemData != null && stackSize + amountToAdd <= itemData.maxStackSize) return true;
        else return false;
    }

    public void AddToStack(int amount)
    {
        stackSize += amount;
    }

    public void RemoveFromStack(int amount)
    {
        stackSize -= amount;
    }

    public bool SplitStack(out InventorySlot splitStack)
    {
        if (stackSize <= 1)
        {
           splitStack = null;
           return false;
        }

        int halfStack = Mathf.RoundToInt(stackSize / 2f);
        RemoveFromStack(halfStack);

        splitStack = new InventorySlot(itemData, halfStack);
        return true;
    }

}