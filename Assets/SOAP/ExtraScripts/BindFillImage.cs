using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class BindFillImage : MonoBehaviour
{
    public enum VariableType
    {
        Float,
        Int
    }

    [Header("Type")]
    public VariableType variableType;

    [Header("Float Variables")]
    public SOVariableFloat currentFloat;
    public SOVariableFloat maxFloat;

    [Header("Int Variables")]
    public SOVariableInt currentInt;
    public SOVariableInt maxInt;

    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    private void OnEnable()
    {
        Subscribe();
        UpdateFill();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    void Subscribe()
    {
        if (variableType == VariableType.Float)
        {
            if (currentFloat != null) currentFloat.OnValueChanged += OnFloatChanged;
            if (maxFloat != null) maxFloat.OnValueChanged += OnFloatChanged;
        }
        else
        {
            if (currentInt != null) currentInt.OnValueChanged += OnIntChanged;
            if (maxInt != null) maxInt.OnValueChanged += OnIntChanged;
        }
    }

    void Unsubscribe()
    {
        if (variableType == VariableType.Float)
        {
            if (currentFloat != null) currentFloat.OnValueChanged -= OnFloatChanged;
            if (maxFloat != null) maxFloat.OnValueChanged -= OnFloatChanged;
        }
        else
        {
            if (currentInt != null) currentInt.OnValueChanged -= OnIntChanged;
            if (maxInt != null) maxInt.OnValueChanged -= OnIntChanged;
        }
    }

    void OnFloatChanged(float _) => UpdateFill();
    void OnIntChanged(int _) => UpdateFill();

    void UpdateFill()
    {
        float current = 0f;
        float max = 1f;

        if (variableType == VariableType.Float)
        {
            if (currentFloat == null || maxFloat == null) return;
            current = currentFloat.Value;
            max = maxFloat.Value;
        }
        else
        {
            if (currentInt == null || maxInt == null) return;
            current = currentInt.Value;
            max = maxInt.Value;
        }

        if (max <= 0f) return;

        img.fillAmount = current / max;
    }
}