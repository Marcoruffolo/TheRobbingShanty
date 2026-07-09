using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProceduralGeneration : MonoBehaviour
{
    [System.Serializable]
    private class PrefabEntry
    {
        public GameObject prefab;
        public Transform parentObject;
        public Transform[] spawnPoints;
        public int priority;
        [Min(0f)] public float amount = 1f;
    }

    [SerializeField] private PrefabEntry[] _entries;
    [SerializeField] private float _exclusionRadius = 0.5f;

    private readonly List<GameObject> _spawnedObjects = new List<GameObject>();

    public IReadOnlyList<GameObject> SpawnedObjects => _spawnedObjects;
    public bool HasGenerated { get; private set; }
    public event System.Action GenerationCompleted;

    void Start()
    {
        SpawnRandom();
    }

    public void SpawnRandom()
    {
        _spawnedObjects.Clear();
        if (_entries == null || _entries.Length == 0) 
        {
            CompleteGeneration();
            return;}

        List<(Vector3 position, int priority)> occupied = new List<(Vector3, int)>();

        foreach (PrefabEntry entry in _entries.OrderByDescending(e => e.priority))
        {
            if (entry.prefab == null || entry.spawnPoints == null || entry.spawnPoints.Length == 0)
            {
                Debug.LogWarning($"[ProceduralGen] Entrada con prefab '{entry.prefab?.name ?? "null"}' omitida: prefab o spawnPoints no configurados.", this);
                continue;
            }

            int count = Mathf.FloorToInt(entry.amount);
            float extraChance = entry.amount - count;
            if (Random.value < extraChance) count++;

            if (count == 0)
            {
                Debug.LogWarning($"[ProceduralGen] '{entry.prefab.name}' tiene amount=0, no se genera nada.", this);
                continue;
            }

            List<Vector3> usedByThisEntry = new List<Vector3>();

            for (int i = 0; i < count; i++)
            {
                Transform spawnPoint = entry.spawnPoints
                    .Where(p => p != null
                        && !usedByThisEntry.Contains(p.position)
                        && !occupied.Any(o => Vector3.Distance(o.position, p.position) < _exclusionRadius && o.priority > entry.priority))
                    .OrderBy(_ => Random.value)
                    .FirstOrDefault();

                if (spawnPoint == null)
                {
                    Debug.LogWarning($"[ProceduralGen] '{entry.prefab.name}' (prioridad {entry.priority}): no hay spawnPoints disponibles.", this);
                    break;
                }

                GameObject spawnedObject = entry.parentObject != null
                    ? Instantiate(entry.prefab, spawnPoint.position, spawnPoint.rotation, entry.parentObject)
                    : Instantiate(entry.prefab, spawnPoint.position, spawnPoint.rotation);
                _spawnedObjects.Add(spawnedObject);
                usedByThisEntry.Add(spawnPoint.position);
                occupied.Add((spawnPoint.position, entry.priority));
            }
        }

        CompleteGeneration();
    }

    private void CompleteGeneration()
    {
        HasGenerated = true;
        GenerationCompleted?.Invoke();
    }
}
