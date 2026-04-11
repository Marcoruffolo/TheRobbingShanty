using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class BindToToggle : MonoBehaviour
{
    public SOVariableBool variable;
    public bool twoWay = true;

    private Toggle toggle;
    private bool isUpdating;

    private void Awake()
    {
        toggle = GetComponent<Toggle>();
    }

    private void OnEnable()
    {
        if (variable != null)
            variable.OnValueChanged += UpdateToggle;

        toggle.onValueChanged.AddListener(OnToggleChanged);

        UpdateToggle(variable != null && variable.Value);
    }

    private void OnDisable()
    {
        if (variable != null)
            variable.OnValueChanged -= UpdateToggle;

        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    void UpdateToggle(bool value)
    {
        isUpdating = true;
        toggle.isOn = value;
        isUpdating = false;
    }

    void OnToggleChanged(bool value)
    {
        if (!twoWay || isUpdating || variable == null) return;
        variable.Value = value;
    }
}