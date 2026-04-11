using UnityEngine;

public class FloatVariableListener : MonoBehaviour
{
    public SOVariableFloat variable;

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

    protected virtual void OnValueChanged(float value) { }
}