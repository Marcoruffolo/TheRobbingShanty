using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    [SerializeField] private SOVariableFloat playerHealth;
    [SerializeField] private SOVariableFloat maxHealth;
    [SerializeField] private Image fillImage;

    private void OnEnable()
    {
        playerHealth.OnValueChanged += UpdateDisplay;
        UpdateDisplay(playerHealth.Value);
    }

    private void OnDisable() => playerHealth.OnValueChanged -= UpdateDisplay;

    private void UpdateDisplay(float value)
    {
        if (fillImage != null) fillImage.fillAmount = value / maxHealth.Value;
    }
}
