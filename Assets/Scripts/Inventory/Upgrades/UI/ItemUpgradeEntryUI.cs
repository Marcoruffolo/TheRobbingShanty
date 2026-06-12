using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ItemUpgradeEntryUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private Image background;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = Color.gray;

    public ItemUpgradeData UpgradeData { get; private set; }

    private ItemUpgradeUI _parent;

    public void Setup(ItemUpgradeData data, ItemUpgradeUI parent)
    {
        UpgradeData = data;
        _parent = parent;

        if (icon != null && data.targetItem?.Icon != null)
            icon.sprite = data.targetItem.Icon;

        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        if (background != null)
            background.color = isSelected ? selectedColor : unselectedColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        _parent.SelectUpgrade(this);
    }
}
