using System.Collections.Generic;
using UnityEngine;

public class WifiSpawner : MonoBehaviour
{
    public List<Transform> _wifiSpawnpoints = new();
    public GameObject _wifiStick;

    [SerializeField] private float stickScale = 1f;

    private readonly List<WifiStick> _spawnedSticks = new();
    private readonly List<GameObject> _spawnedObjects = new();

    public void BuildSticks(int requiredCount)
    {
        ClearSticks();

        int count = Mathf.Max(0, requiredCount);
        if (count == 0) return;

        if (_wifiStick == null)
        {
            Debug.LogError("[WifiSpawner] Falta _wifiStick para crear marcadores de puerta.", this);
            return;
        }

        int availableSpawnpoints = CountValidSpawnpoints();
        if (availableSpawnpoints == 0)
        {
            Debug.LogError("[WifiSpawner] No hay WifiSpawnPoints validos para crear marcadores de puerta.", this);
            return;
        }

        if (count > availableSpawnpoints)
            Debug.LogError($"[WifiSpawner] La puerta necesita {count} sticks, pero solo hay {availableSpawnpoints} WifiSpawnPoints validos.", this);

        Transform stickParent = GetStickParent();
        Vector3 worldScale = Vector3.one * Mathf.Max(0.01f, stickScale);
        int spawnCount = Mathf.Min(count, availableSpawnpoints);
        for (int i = 0; i < spawnCount; i++)
        {
            Transform spawnpoint = GetValidSpawnpoint(i);
            GameObject stickObject = Instantiate(_wifiStick, spawnpoint.position, spawnpoint.rotation, stickParent);
            stickObject.name = $"WifiStick_{i + 1}";
            SetWorldScale(stickObject.transform, worldScale);

            WifiStick stick = stickObject.GetComponentInChildren<WifiStick>(true);
            if (stick == null)
                Debug.LogError("[WifiSpawner] El prefab asignado no tiene componente WifiStick.", stickObject);
            else
                stick.SetTurnedOn(false);

            _spawnedObjects.Add(stickObject);
            _spawnedSticks.Add(stick);
        }
    }

    public void SetLitCount(int litCount)
    {
        int lit = Mathf.Clamp(litCount, 0, _spawnedSticks.Count);
        for (int i = 0; i < _spawnedSticks.Count; i++)
        {
            WifiStick stick = _spawnedSticks[i];
            if (stick != null)
                stick.SetTurnedOn(i < lit);
        }
    }

    public void ClearSticks()
    {
        foreach (GameObject stickObject in _spawnedObjects)
        {
            if (stickObject == null) continue;

            if (Application.isPlaying)
                Destroy(stickObject);
            else
                DestroyImmediate(stickObject);
        }

        _spawnedObjects.Clear();
        _spawnedSticks.Clear();
    }

    private int CountValidSpawnpoints()
    {
        int count = 0;
        if (_wifiSpawnpoints == null) return count;

        foreach (Transform spawnpoint in _wifiSpawnpoints)
            if (spawnpoint != null)
                count++;

        return count;
    }

    private Transform GetValidSpawnpoint(int validIndex)
    {
        int current = 0;
        foreach (Transform spawnpoint in _wifiSpawnpoints)
        {
            if (spawnpoint == null) continue;
            if (current == validIndex) return spawnpoint;
            current++;
        }

        return null;
    }

    private Transform GetStickParent()
    {
        return transform.parent != null ? transform.parent : transform;
    }

    private void SetWorldScale(Transform target, Vector3 worldScale)
    {
        Transform parent = target.parent;
        if (parent == null)
        {
            target.localScale = worldScale;
            return;
        }

        Vector3 parentScale = parent.lossyScale;
        target.localScale = new Vector3(
            SafeDivide(worldScale.x, parentScale.x),
            SafeDivide(worldScale.y, parentScale.y),
            SafeDivide(worldScale.z, parentScale.z));
    }

    private float SafeDivide(float value, float divisor)
    {
        return Mathf.Approximately(divisor, 0f) ? value : value / divisor;
    }
}