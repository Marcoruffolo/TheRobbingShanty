using UnityEngine;
using TMPro;

public class ShipDurabilityBar : MonoBehaviour
{
    [SerializeField] private SOVariableFloat durability;
    [SerializeField] private TMP_Text label;
    [SerializeField] private float maxDurability = 100f;

    private void OnEnable()
    {
        durability.OnValueChanged += UpdateDisplay;
        UpdateDisplay(durability.Value);
    }

    private void OnDisable() => durability.OnValueChanged -= UpdateDisplay;

    private void UpdateDisplay(float value) => label.text = $"{Mathf.CeilToInt(value)}/{Mathf.RoundToInt(maxDurability)}";
}
