using UnityEngine;

[CreateAssetMenu(menuName = "SOAP/Variable/Int")]
public class SOVariableInt : ScriptableVariable<int>
{
    [Header("Clamp")]
    public bool IsClamped;
    public int Min;
    public int Max;

    public override void SetValue(int newValue)
    {
        if (IsClamped)
            newValue = Mathf.Clamp(newValue, Min, Max);

        base.SetValue(newValue);
    }

    public void Add(int amount) => SetValue(Value + amount);
    public void Subtract(int amount) => SetValue(Value - amount);
}