using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : Component
{
    private readonly Queue<T> _available = new();

    public ObjectPool(Func<T> factory, int size)
    {
        for (int i = 0; i < size; i++)
        {
            T instance = factory();
            instance.gameObject.SetActive(false);
            _available.Enqueue(instance);
        }
    }

    public bool TryGet(out T instance)
    {
        if (_available.Count == 0)
        {
            instance = null;
            return false;
        }

        instance = _available.Dequeue();
        instance.gameObject.SetActive(true);
        return true;
    }

    public void Return(T instance)
    {
        instance.gameObject.SetActive(false);
        _available.Enqueue(instance);
    }
}
