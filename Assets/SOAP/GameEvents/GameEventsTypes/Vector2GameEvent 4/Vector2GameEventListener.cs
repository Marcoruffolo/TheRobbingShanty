using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Vector2UnityEvent : UnityEvent<Vector2> { }

public class Vector2GameEventListener : GameEventListener<Vector2, Vector2GameEvent>
{
    [SerializeField] private Vector2UnityEvent response;

    public override void OnEventRaised(Vector2 value)
    {
        response?.Invoke(value);
    }
}