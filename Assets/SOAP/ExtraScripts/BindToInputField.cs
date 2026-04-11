using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class BindToInputField : MonoBehaviour
{
    public SOVariableString variable;
    public bool twoWay = true;

    private TMP_InputField input;
    private bool isUpdating;

    private void Awake()
    {
        input = GetComponent<TMP_InputField>();
    }

    private void OnEnable()
    {
        if (variable != null)
            variable.OnValueChanged += UpdateInput;

        input.onValueChanged.AddListener(OnInputChanged);

        UpdateInput(variable != null ? variable.Value : "");
    }

    private void OnDisable()
    {
        if (variable != null)
            variable.OnValueChanged -= UpdateInput;

        input.onValueChanged.RemoveListener(OnInputChanged);
    }

    void UpdateInput(string value)
    {
        isUpdating = true;
        input.text = value;
        isUpdating = false;
    }

    void OnInputChanged(string value)
    {
        if (!twoWay || isUpdating || variable == null) return;
        variable.Value = value;
    }
}