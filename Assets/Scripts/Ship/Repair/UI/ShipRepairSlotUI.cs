using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ShipRepairSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemCountText;

    [SerializeField] private Color sufficientColor = Color.green;
    [SerializeField] private Color insufficientColor = Color.red;

    public RepairRequirement Requirement { get; private set; }
    public bool IsFulfilled => Requirement != null && Requirement.Deposited >= Requirement.requiredAmount;

    public UnityAction<ShipRepairSlotUI> OnItemDeposited;

    private MouseItemData _mouseItemData;

    public void Setup(RepairRequirement requirement, MouseItemData mouseItemData)
    {
        Requirement = requirement;
        _mouseItemData = mouseItemData;
        RefreshDisplay();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Requirement == null) return;
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (IsFulfilled) return;
        if (_mouseItemData.AssignedInventorySlot.ItemData != Requirement.itemData) return;

        _mouseItemData.AssignedInventorySlot.RemoveFromStack(1);

        if (_mouseItemData.AssignedInventorySlot.StackSize <= 0)
            _mouseItemData.ClearSlot();
        else
            _mouseItemData.itemCount.text = _mouseItemData.AssignedInventorySlot.StackSize.ToString();

        Requirement.Deposited++;
        RefreshDisplay();
        OnItemDeposited?.Invoke(this);
    }

    public void Refresh() => RefreshDisplay();

    private void RefreshDisplay()
    {
        if (Requirement.itemData.Icon != null)
            itemIcon.sprite = Requirement.itemData.Icon;

        itemCountText.text = $"{Requirement.Deposited} / {Requirement.requiredAmount}";
        itemCountText.color = IsFulfilled ? sufficientColor : insufficientColor;
    }
}
