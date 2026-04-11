using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class StringUnityEvent : UnityEvent<string> { }

public class StringGameEventListener : GameEventListener<string, StringGameEvent>
{
    [SerializeField] private StringUnityEvent response;

    public override void OnEventRaised(string value)
    {
        response?.Invoke(value);
    }
}