using UnityEngine;

public class StringVariableListener : MonoBehaviour
{
    public SOVariableString variable;

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

    protected virtual void OnValueChanged(string value) { }
}