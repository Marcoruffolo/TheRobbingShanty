using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class DynamicInventoryDisplay : InventoryDisplay
{
    [SerializeField] protected InventorySlot_UI slotPrefab;

    protected override void Start()
    {
        base.Start();
    }

    public void RefreshDynamicInventory(InventorySystem invToDisplay)
    {
        ClearSlots();
        inventorySystem = invToDisplay;
        if(inventorySystem != null) inventorySystem.OnInventorySlotChanged += UpdateSlot;
        AssignSlot(invToDisplay);
    }

    public override void AssignSlot(InventorySystem invToDisplay)
    {
        ClearSlots();

        slotDictionary = new Dictionary<InventorySlot_UI, InventorySlot>();

        if(invToDisplay == null) return;

        for (int i = 0; i < invToDisplay.InventorySize; i++)
        {
            var uiSlot = Instantiate(slotPrefab, transform);
            slotDictionary.Add(uiSlot, invToDisplay.InventorySlots[i]);
            uiSlot.Init(invToDisplay.InventorySlots[i]);
            uiSlot.UpdateUISlot();
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
                    inventorySystem?.OnInventorySlotChanged?.Invoke(clickedUISlot.AssignedInventorySlot);
                }
            }
            return;
        }

        if (usingMouse) return;

        var playerHolder = PlayerInventoryHolder.Instance;
        if (playerHolder == null) return;

        var itemData = clickedUISlot.AssignedInventorySlot.ItemData;
        int amount = clickedUISlot.AssignedInventorySlot.StackSize;

        if (playerHolder.PrimaryInventorySystem.AddToInventory(itemData, amount))
        {
            clickedUISlot.AssignedInventorySlot.ClearSlot();
            clickedUISlot.UpdateUISlot();
            inventorySystem?.OnInventorySlotChanged?.Invoke(clickedUISlot.AssignedInventorySlot);
        }
    }

    private void ClearSlots()
    {
        foreach (var item in transform.Cast<Transform>())
        {
            Destroy(item.gameObject);
        }

        if (slotDictionary != null) slotDictionary.Clear();
    }

    private void OnDisable()
    {
        if(inventorySystem != null) inventorySystem.OnInventorySlotChanged -= UpdateSlot;
    }
}
