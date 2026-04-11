using UnityEngine;
using UnityEngine.Events;

public class BindComparisonToUnityEvent : MonoBehaviour
{
    public enum ComparisonType
    {
        Greater,
        Less,
        Equal,
        GreaterOrEqual,
        LessOrEqual
    }

    public SOVariableFloat variable;
    public float compareTo;
    public ComparisonType comparison;

    public UnityEvent OnTrue;
    public UnityEvent OnFalse;

    private void OnEnable()
    {
        if (variable != null)
            variable.OnValueChanged += Evaluate;

        Evaluate(variable != null ? variable.Value : 0);
    }

    private void OnDisable()
    {
        if (variable != null)
            variable.OnValueChanged -= Evaluate;
    }

    void Evaluate(float value)
    {
        bool result = comparison switch
        {
            ComparisonType.Greater => value > compareTo,
            ComparisonType.Less => value < compareTo,
            ComparisonType.Equal => Mathf.Approximately(value, compareTo),
            ComparisonType.GreaterOrEqual => value >= compareTo,
            ComparisonType.LessOrEqual => value <= compareTo,
            _ => false
        };

        if (result) OnTrue?.Invoke();
        else OnFalse?.Invoke();
    }
}