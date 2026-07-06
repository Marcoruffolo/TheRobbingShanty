using System.Collections.Generic;
using UnityEngine;

public class StaticInventoryDisplay : InventoryDisplay
{
    [SerializeField] private InventoryHolder inventoryHolder;
    [SerializeField] private InventorySlot_UI[] slots;

    int _currentIndex = 0;

    protected override void Start()
    {
        base.Start();

        if (inventoryHolder == null)
            inventoryHolder = PlayerInventoryHolder.Instance;

        if (inventoryHolder != null)
        {
            inventorySystem = inventoryHolder.PrimaryInventorySystem;
            inventorySystem.OnInventorySlotChanged += UpdateSlot;
        }
        else Debug.LogWarning("InventoryHolder is not assigned!");

        AssignSlot(inventorySystem);
    }

    public override void AssignSlot(InventorySystem invToDisplay)
    {
        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();

        if (slots.Length != inventorySystem.InventorySize) Debug.LogError("Slot count does not match inventory size!");

        for (int i = 0; i < inventorySystem.InventorySize; i++)
        {
            slotDictionary.Add(slots[i], inventorySystem.InventorySlots[i]);
            slots[i].Init(inventorySystem.InventorySlots[i]);
        }
    }

    public override void SlotRightClicked(InventorySlot_UI clickedUISlot)
    {
        var mouseSlot = MouseItem?.AssignedInventorySlot;
        bool usingMouse = mouseSlot?.ItemData != null;
        var source = usingMouse ? mouseSlot : clickedUISlot.AssignedInventorySlot;
        SetIndex(System.Array.IndexOf(slots, clickedUISlot));

        if (source.ItemData == null) return;

        if (RepairDepositRouter.HasActiveHandler)
        {
            if (RepairDepositRouter.TryDeposit(source, inventorySystem))
            {
                if (usingMouse)
                    MouseItem.ClearSlot();
                else
                {
                    clickedUISlot.UpdateUISlot();
                    inventorySystem.OnInventorySlotChanged?.Invoke(clickedUISlot.AssignedInventorySlot);
                }
            }
            return;
        }

        if (usingMouse) return;

        var playerHolder = PlayerInventoryHolder.Instance;
        if (playerHolder == null || !playerHolder.IsBackpackOpen) return;

        var itemData = clickedUISlot.AssignedInventorySlot.ItemData;
        int amount = clickedUISlot.AssignedInventorySlot.StackSize;

        if (playerHolder.SecondaryInventorySystem.AddToInventory(itemData, amount))
        {
            clickedUISlot.AssignedInventorySlot.ClearSlot();
            clickedUISlot.UpdateUISlot();
            inventorySystem.OnInventorySlotChanged?.Invoke(clickedUISlot.AssignedInventorySlot);
        }
    }

    public void SetIndex(int i)
    {
        if(i < 0) _currentIndex = 0;
        if(i > (slots.Length - 1)) _currentIndex = slots.Length - 1;
        _currentIndex = i;
    }

    private void UseItem()
    {
        if(slots[_currentIndex].AssignedInventorySlot.ItemData != null) slots[_currentIndex].AssignedInventorySlot.ItemData.UseItem();
    }
}
