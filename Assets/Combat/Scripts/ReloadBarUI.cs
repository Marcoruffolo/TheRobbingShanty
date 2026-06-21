using UnityEngine;
using UnityEngine.UI;

public class ReloadBarUI : MonoBehaviour
{
    [SerializeField] private SOVariableFloat reloadProgress;
    [SerializeField] private Image fillImage;

    private void OnEnable()
    {
        reloadProgress.OnValueChanged += UpdateDisplay;
        UpdateDisplay(reloadProgress.Value);
    }

    private void OnDisable() => reloadProgress.OnValueChanged -= UpdateDisplay;

    private void UpdateDisplay(float value)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = value;
        fillImage.enabled = value < 1f;
    }
}
