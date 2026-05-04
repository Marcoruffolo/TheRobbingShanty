using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShipRepairUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private List<ShipRepairSlotUI> slots;
    [SerializeField] private MouseItemData mouseItemData;

    public static UnityAction OnCloseRequested;

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

        panel.SetActive(false);
        _repairData = null;
    }

    private void CheckRepairComplete(ShipRepairSlotUI _)
    {
        foreach (var slot in slots)
            if (slot.gameObject.activeSelf && !slot.IsFulfilled) return;

        OnCloseRequested?.Invoke();
    }
}
