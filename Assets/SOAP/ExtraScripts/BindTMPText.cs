using TMPro;
using UnityEngine;

public class BindTMPText : MonoBehaviour
{
    public enum VariableType
    {
        Float,
        Int,
        String
    }

    public VariableType variableType;

    public string prefix;
    public string suffix;

    [Header("References")]
    public SOVariableFloat floatVar;
    public SOVariableInt intVar;
    public SOVariableString stringVar;

    [Header("Float Settings")]
    public int decimalAmount = 2;

    private TMP_Text text;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        Subscribe();
        UpdateText();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    void Subscribe()
    {
        if (floatVar != null) floatVar.OnValueChanged += OnFloatChanged;
        if (intVar != null) intVar.OnValueChanged += OnIntChanged;
        if (stringVar != null) stringVar.OnValueChanged += OnStringChanged;
    }

    void Unsubscribe()
    {
        if (floatVar != null) floatVar.OnValueChanged -= OnFloatChanged;
        if (intVar != null) intVar.OnValueChanged -= OnIntChanged;
        if (stringVar != null) stringVar.OnValueChanged -= OnStringChanged;
    }

    void OnFloatChanged(float _) => UpdateText();
    void OnIntChanged(int _) => UpdateText();
    void OnStringChanged(string _) => UpdateText();

    void UpdateText()
    {
        string value = "";

        switch (variableType)
        {
            case VariableType.Float:
                value = floatVar != null ? floatVar.Value.ToString($"F{decimalAmount}") : "0";
                break;

            case VariableType.Int:
                value = intVar != null ? intVar.Value.ToString() : "0";
                break;

            case VariableType.String:
                value = stringVar != null ? stringVar.Value : "";
                break;
        }

        text.text = $"{prefix}{value}{suffix}";
    }
}