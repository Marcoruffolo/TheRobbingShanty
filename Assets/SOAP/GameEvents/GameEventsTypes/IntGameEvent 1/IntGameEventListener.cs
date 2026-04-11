using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntUnityEvent : UnityEvent<int> { }

public class IntGameEventListener : GameEventListener<int, IntGameEvent>
{
    [SerializeField] private IntUnityEvent response;

    public override void OnEventRaised(int value)
    {
        response?.Invoke(value);
    }
}