using UnityEngine;

[CreateAssetMenu(menuName = "SOAP/Events/GameEvent")]
public class VoidGameEvent : GameEvent<VoidType>
{
    public void Raise()
    {
        Raise(default);
    }
}

// helper type
[System.Serializable]
public struct VoidType { }