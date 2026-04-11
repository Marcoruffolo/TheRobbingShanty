using UnityEngine;

[CreateAssetMenu(menuName = "SOAP/Variable/Float")]
public class SOVariableFloat : ScriptableVariable<float>
{
    public bool IsClamped;
    public float Min;
    public float Max;

    public override void SetValue(float newValue)
    {
        if (IsClamped)
            newValue = Mathf.Clamp(newValue, Min, Max);

        base.SetValue(newValue);
    }

    public void Add(float amount) => SetValue(Value + amount);
}