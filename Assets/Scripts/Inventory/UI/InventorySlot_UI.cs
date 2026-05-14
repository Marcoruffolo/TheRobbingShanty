using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class InventorySlot_UI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemSprite;
    [SerializeField] private TextMeshProUGUI itemCount;
    [SerializeField] private InventorySlot assignedInventorySlot;

    private Button button;
    private float _lastClickTime;
    private const float DoubleClickThreshold = 0.3f;

    public InventorySlot AssignedInventorySlot => assignedInventorySlot;
    public InventoryDisplay ParentDisplay { get; private set; }

    private void Awake() {

        ClearSlot();

        button = GetComponent<Button>();

        ParentDisplay = transform.parent.GetComponent<InventoryDisplay>();
    }

    public void Init(InventorySlot slot)
    {
        assignedInventorySlot = slot;
        UpdateUISlot(slot);
    }

    public void UpdateUISlot(InventorySlot slot)
    {
        if (slot.ItemData != null)
        {
            itemSprite.sprite = slot.ItemData.Icon;
            itemSprite.color = Color.white;

            if (slot.StackSize > 1) itemCount.text = slot.StackSize.ToString();
            else itemCount.text = "";
        }
        else
        {
            ClearSlot();
        }
    }

    public void UpdateUISlot()
    {
        if (assignedInventorySlot != null)
        {
            UpdateUISlot(assignedInventorySlot);
        }
    }

    public void ClearSlot()
    {
        assignedInventorySlot?.ClearSlot();
        itemSprite.sprite = null;
        itemSprite.color = Color.clear;
        itemCount.text = "";
    }

    public void OnUISlotClick()
    {
        ParentDisplay.SlotClicked(this);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            ParentDisplay.SlotRightClicked(this);
            return;
        }

        if (eventData.button != PointerEventData.InputButton.Left) return;

        float now = Time.unscaledTime;
        bool isDoubleClick = (now - _lastClickTime) <= DoubleClickThreshold;
        _lastClickTime = now;

        if (isDoubleClick && RepairDepositRouter.HasActiveHandler)
        {
            MouseItemData mouseItem = ParentDisplay.MouseItem;
            InventorySlot depositSource = mouseItem?.AssignedInventorySlot.ItemData != null
                ? mouseItem.AssignedInventorySlot
                : assignedInventorySlot;

            if (depositSource?.ItemData != null && RepairDepositRouter.TryDeposit(depositSource))
            {
                if (depositSource == mouseItem?.AssignedInventorySlot)
                {
                    if (depositSource.StackSize <= 0)
                        mouseItem.ClearSlot();
                    else
                        mouseItem.itemCount.text = depositSource.StackSize.ToString();
                }
                else
                {
                    UpdateUISlot();
                    ParentDisplay.InventorySystem?.OnInventorySlotChanged?.Invoke(assignedInventorySlot);
                }
                return;
            }
        }

        ParentDisplay.SlotClicked(this);
    }
}
