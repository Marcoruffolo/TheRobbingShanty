using UnityEngine;

[CreateAssetMenu(menuName = "SOAP/Variable/Bool")]
public class SOVariableBool : ScriptableVariable<bool>
{
    public void Toggle() => SetValue(!Value);
}