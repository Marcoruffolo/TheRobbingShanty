using UnityEngine;

public class BoolVariableListener : MonoBehaviour
{
    public SOVariableBool variable;

    private void OnEnable()
    {
        if (variable != null)
            variable.OnValueChanged += OnValueChanged;
    }

    private void OnDisable()
    {
        if (variable != null)
            variable.OnValueChanged -= OnValueChanged;
    }

    protected virtual void OnValueChanged(bool value) { }
}