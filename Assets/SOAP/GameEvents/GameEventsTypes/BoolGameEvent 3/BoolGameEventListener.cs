using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class BoolUnityEvent : UnityEvent<bool> { }

public class BoolGameEventListener : GameEventListener<bool, BoolGameEvent>
{
    [SerializeField] private BoolUnityEvent response;

    public override void OnEventRaised(bool value)
    {
        response?.Invoke(value);
    }
}