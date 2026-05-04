using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShipDurabilitySlotUI : MonoBehaviour, IPointerClickHandler
{
    public UnityAction OnWoodDeposited;

    private Image _image;
    private MouseItemData _mouseItemData;
    private InventoryItemData _woodItemData;
    private SOVariableFloat _durability;

    private void Awake()
    {
        _image = GetComponent<Image>();
        if (_image != null) _image.raycastTarget = true;
    }

    public void Setup(InventoryItemData woodItemData, MouseItemData mouseItemData, SOVariableFloat durability)
    {
        _woodItemData = woodItemData;
        _mouseItemData = mouseItemData;
        _durability = durability;

        if (_image != null && woodItemData.Icon != null)
            _image.sprite = woodItemData.Icon;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (_mouseItemData.AssignedInventorySlot.ItemData != _woodItemData) return;
        if (_durability.IsClamped && _durability.Value >= _durability.Max) return;

        _mouseItemData.AssignedInventorySlot.RemoveFromStack(1);

        if (_mouseItemData.AssignedInventorySlot.StackSize <= 0)
            _mouseItemData.ClearSlot();
        else
            _mouseItemData.itemCount.text = _mouseItemData.AssignedInventorySlot.StackSize.ToString();

        OnWoodDeposited?.Invoke();
    }
}
