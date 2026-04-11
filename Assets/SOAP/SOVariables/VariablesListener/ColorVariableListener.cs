using UnityEngine;

public class ColorVariableListener : MonoBehaviour
{
    public SOVariableColor variable;

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

    protected virtual void OnValueChanged(Color value) { }
}