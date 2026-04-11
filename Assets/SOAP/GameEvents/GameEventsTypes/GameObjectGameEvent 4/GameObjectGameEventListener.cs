using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class GameObjectUnityEvent : UnityEvent<GameObject> { }

public class GameObjectGameEventListener : GameEventListener<GameObject, GameObjectGameEvent>
{
    [SerializeField] private GameObjectUnityEvent response;

    public override void OnEventRaised(GameObject value)
    {
        response?.Invoke(value);
    }
}