using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShipRepairUI : MonoBehaviour, IItemDepositHandler
{
    [SerializeField] private GameObject panel;
    [SerializeField] private List<ShipRepairSlotUI> slots;
    [SerializeField] private MouseItemData mouseItemData;

    public static UnityAction OnCloseRequested;
    public static UnityAction OnRepairCompleted;

    private ShipRepairData _repairData;

    private void Awake()
    {
        panel.SetActive(false);
    }

    private void OnEnable()
    {
        ShipRepairInteractable.OnRepairUIOpened += Open;
        ShipRepairInteractable.OnRepairUIClosed += Close;
    }

    private void OnDisable()
    {
        ShipRepairInteractable.OnRepairUIOpened -= Open;
        ShipRepairInteractable.OnRepairUIClosed -= Close;
    }

    private void Open(ShipRepairData data)
    {
        _repairData = data;
        panel.SetActive(true);
        RepairDepositRouter.Register(this);

        for (int i = 0; i < slots.Count; i++)
        {
            if (i < _repairData.requirements.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Setup(_repairData.requirements[i], mouseItemData);
                slots[i].OnItemDeposited += CheckRepairComplete;
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    private void Close()
    {
        foreach (var slot in slots)
            slot.OnItemDeposited -= CheckRepairComplete;

        RepairDepositRouter.Unregister();
        panel.SetActive(false);
        _repairData = null;
    }

    public bool TryDeposit(InventorySlot sourceSlot)
    {
        if (_repairData == null || sourceSlot?.ItemData == null) return false;

        foreach (var req in _repairData.requirements)
        {
            if (req.itemData != sourceSlot.ItemData) continue;
            if (req.Deposited >= req.requiredAmount) continue;

            int toDeposit = Mathf.Min(sourceSlot.StackSize, req.requiredAmount - req.Deposited);
            if (toDeposit <= 0) continue;

            sourceSlot.RemoveFromStack(toDeposit);
            if (sourceSlot.StackSize <= 0) sourceSlot.ClearSlot();

            req.Deposited += toDeposit;

            foreach (var slotUI in slots)
            {
                if (slotUI.gameObject.activeSelf && slotUI.Requirement == req)
                {
                    slotUI.Refresh();
                    break;
                }
            }

            CheckRepairComplete(null);
            return true;
        }

        return false;
    }

    private void CheckRepairComplete(ShipRepairSlotUI _)
    {
        foreach (var slot in slots)
            if (slot.gameObject.activeSelf && !slot.IsFulfilled) return;

        OnRepairCompleted?.Invoke();
        OnCloseRequested?.Invoke();
    }
}
