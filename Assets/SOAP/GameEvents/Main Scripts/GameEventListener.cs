using UnityEngine;

public abstract class GameEventListener<T, E> : MonoBehaviour, IGameEventListener<T>
    where E : GameEvent<T>
{
    [SerializeField] private E gameEvent;

    protected virtual void OnEnable()
    {
        if (gameEvent != null)
            gameEvent.RegisterListener(this);
    }

    protected virtual void OnDisable()
    {
        if (gameEvent != null)
            gameEvent.UnregisterListener(this);
    }

    public abstract void OnEventRaised(T value);
}