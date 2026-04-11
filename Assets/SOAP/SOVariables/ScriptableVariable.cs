using System;
using UnityEngine;

public abstract class ScriptableVariable<T> : ScriptableObject
{
    [SerializeField] protected T value;
    public virtual T Value
    {
        get => value;
        set => SetValue(value);
    }

    [Header("Initial Value")]
    public T InitialValue;

    [Header("Debug")]
    public bool DebugLogEnabled;

    [Header("Reset")]
    public StateToLoad[] ResetOn = new StateToLoad[] { StateToLoad.None };

    public Action<T> OnValueChanged;

    public enum StateToLoad
    {
        None,
        Awake,
        OnEnable,
        OnDisable,
        SceneLoaded,
        SceneEnabled,
        Manual
    }

    // ------------------------
    // CORE
    // ------------------------

    public virtual void SetValue(T newValue)
    {
        T oldValue = value;

        value = newValue;

        if (DebugLogEnabled)
            Debug.Log($"[{name}] {oldValue} → {value}");

        OnValueChanged?.Invoke(value);
    }

    public virtual void ResetValue()
    {
        if (DebugLogEnabled)
            Debug.Log($"[{name}] Reset → {InitialValue}");

        SetValue(InitialValue);
    }

    // ------------------------
    // LIFECYCLE
    // ------------------------

    protected virtual void OnEnable() => TryReset(StateToLoad.OnEnable);
    protected virtual void Awake() => TryReset(StateToLoad.Awake);

    public void OnSceneLoaded() => TryReset(StateToLoad.SceneLoaded);
    public void OnSceneEnabled() => TryReset(StateToLoad.SceneEnabled);
    public void OnDisable() => TryReset(StateToLoad.OnDisable);

    protected void TryReset(StateToLoad state)
    {
        if (ResetOn == null) return;

        foreach (var s in ResetOn)
        {
            if (s == state)
            {
                ResetValue();
                return;
            }
        }
    }
}