using UnityEngine;
using TMPro;

public class ShipDurabilityBar : MonoBehaviour
{
    [SerializeField] private SOVariableFloat durability;
    [SerializeField] private TMP_Text label;

    private void OnEnable()
    {
        durability.OnValueChanged += UpdateDisplay;
        ShipUpgradeManager.OnUpgradeApplied += HandleUpgradeApplied;
        UpdateDisplay(durability.Value);
    }

    private void OnDisable()
    {
        durability.OnValueChanged -= UpdateDisplay;
        ShipUpgradeManager.OnUpgradeApplied -= HandleUpgradeApplied;
    }

    private void HandleUpgradeApplied(ShipUpgradeData data) => UpdateDisplay(durability.Value);

    private void UpdateDisplay(float value) => label.text = $"{Mathf.CeilToInt(value)}/{Mathf.RoundToInt(durability.Max)}";
}
