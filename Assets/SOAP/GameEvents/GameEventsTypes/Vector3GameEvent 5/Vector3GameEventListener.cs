using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Vector3UnityEvent : UnityEvent<Vector3> { }

public class Vector3GameEventListener : GameEventListener<Vector3, Vector3GameEvent>
{
    [SerializeField] private Vector3UnityEvent response;

    public override void OnEventRaised(Vector3 value)
    {
        response?.Invoke(value);
    }
}