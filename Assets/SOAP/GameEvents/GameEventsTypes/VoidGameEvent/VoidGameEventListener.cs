using UnityEngine;
using UnityEngine.Events;

public class VoidGameEventListener : GameEventListener<VoidType, VoidGameEvent>
{
    [SerializeField] private UnityEvent response;

    public override void OnEventRaised(VoidType value)
    {
        response?.Invoke();
    }
}