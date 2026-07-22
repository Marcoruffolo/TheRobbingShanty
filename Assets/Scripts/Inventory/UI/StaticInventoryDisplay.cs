using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StaticInventoryDisplay : InventoryDisplay
{
    [SerializeField] private InventoryHolder inventoryHolder;
    [SerializeField] private InventorySlot_UI[] slots;

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

        PlayerInventoryHolder.OnHotbarIndexChanged += HighlightIndex;

        // Sync to whatever index is already selected on start
        if (inventoryHolder is PlayerInventoryHolder playerHolder)
            HighlightIndex(playerHolder.SelectedHotbarIndex);
    }

    private void OnDestroy()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventorySlotChanged -= UpdateSlot;

        PlayerInventoryHolder.OnHotbarIndexChanged -= HighlightIndex;
    }

    private void HighlightIndex(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        SelectSlot(slots[index]);
    }

    public override void SelectSlot(InventorySlot_UI clickedUISlot)
    {
        base.SelectSlot(clickedUISlot);

        int index = Array.IndexOf(slots, clickedUISlot);
        if (index >= 0 && PlayerInventoryHolder.Instance != null)
            PlayerInventoryHolder.Instance.SelectHotbarIndex(index);
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
}