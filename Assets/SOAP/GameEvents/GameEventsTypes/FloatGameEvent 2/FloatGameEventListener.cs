using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class FloatUnityEvent : UnityEvent<float> { }

public class FloatGameEventListener : GameEventListener<float, FloatGameEvent>
{
    [SerializeField] private FloatUnityEvent response;

    public override void OnEventRaised(float value)
    {
        response?.Invoke(value);
    }
}