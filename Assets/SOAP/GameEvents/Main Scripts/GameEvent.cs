using System.Collections.Generic;
using UnityEngine;

public abstract class GameEvent<T> : ScriptableObject
{
    private readonly List<IGameEventListener<T>> listeners = new();

    [Header("Debug")]
    public bool DebugLogEnabled;

    public void Raise(T value)
    {
        // iterate backwards (safe removal)
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            if (DebugLogEnabled)
                Debug.Log($"[{name}] Raised with value: {value}");
            listeners[i].OnEventRaised(value);
        }
    }

    public void RegisterListener(IGameEventListener<T> listener)
    {
        if (!listeners.Contains(listener))
            listeners.Add(listener);
    }

    public void UnregisterListener(IGameEventListener<T> listener)
    {
        if (listeners.Contains(listener))
            listeners.Remove(listener);
    }
}
//SOAP/Events/Int Event