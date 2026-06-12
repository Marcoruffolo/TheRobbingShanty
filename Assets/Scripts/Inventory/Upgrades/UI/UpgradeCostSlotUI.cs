using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeCostSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text amountText;
    [SerializeField] private Color sufficientColor = Color.green;
    [SerializeField] private Color insufficientColor = Color.red;

    public void Setup(UpgradeCost cost, int availableAmount)
    {
        if (icon != null && cost.material?.Icon != null)
            icon.sprite = cost.material.Icon;

        amountText.text = $"{availableAmount} / {cost.amount}";
        amountText.color = availableAmount >= cost.amount ? sufficientColor : insufficientColor;
    }
}
