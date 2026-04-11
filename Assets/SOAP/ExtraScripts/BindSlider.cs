using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class BindSlider : MonoBehaviour
{
    public enum VariableType
    {
        Float,
        Int
    }

    [Header("Type")]
    public VariableType variableType;

    [Header("Float")]
    public SOVariableFloat floatVariable;

    [Header("Int")]
    public SOVariableInt intVariable;

    [Header("Settings")]
    public bool twoWay = true;
    public bool wholeNumbers = false; // useful for int sliders

    private Slider slider;
    private bool isUpdating;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        slider.wholeNumbers = (variableType == VariableType.Int) || wholeNumbers;

        Subscribe();
        slider.onValueChanged.AddListener(OnSliderChanged);

        UpdateSlider();
    }

    private void OnDisable()
    {
        Unsubscribe();
        slider.onValueChanged.RemoveListener(OnSliderChanged);
    }

    void Subscribe()
    {
        if (variableType == VariableType.Float)
        {
            if (floatVariable != null)
                floatVariable.OnValueChanged += OnFloatChanged;
        }
        else
        {
            if (intVariable != null)
                intVariable.OnValueChanged += OnIntChanged;
        }
    }

    void Unsubscribe()
    {
        if (variableType == VariableType.Float)
        {
            if (floatVariable != null)
                floatVariable.OnValueChanged -= OnFloatChanged;
        }
        else
        {
            if (intVariable != null)
                intVariable.OnValueChanged -= OnIntChanged;
        }
    }

    void OnFloatChanged(float _) => UpdateSlider();
    void OnIntChanged(int _) => UpdateSlider();

    void UpdateSlider()
    {
        isUpdating = true;

        if (variableType == VariableType.Float)
        {
            slider.value = floatVariable != null ? floatVariable.Value : 0f;
        }
        else
        {
            slider.value = intVariable != null ? intVariable.Value : 0f;
        }

        isUpdating = false;
    }

    void OnSliderChanged(float value)
    {
        if (!twoWay || isUpdating) return;

        if (variableType == VariableType.Float)
        {
            if (floatVariable != null)
                floatVariable.Value = value;
        }
        else
        {
            if (intVariable != null)
                intVariable.Value = Mathf.RoundToInt(value);
        }
    }
}