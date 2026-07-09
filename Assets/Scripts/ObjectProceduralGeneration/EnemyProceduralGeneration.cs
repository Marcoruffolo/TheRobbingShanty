using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyProceduralGeneration : MonoBehaviour
{
    [System.Serializable]
    private class EnemyEntry
    {
        public GameObject prefab;
        public Transform[] spawnPoints;
        public int priority;
        [Min(0f)] public float amount = 1f;
    }

    [SerializeField] private EnemyEntry[] _entries;
    [SerializeField] private float _exclusionRadius = 0.5f;

    [Header("NavMesh Validation")]
    [Tooltip("Distancia máxima para buscar NavMesh cerca del spawn point")]
    [SerializeField] private float _navMeshSampleDistance = 2f;
    [Tooltip("Máscara de áreas de NavMesh válidas (por defecto todas)")]
    [SerializeField] private int _navMeshAreaMask = NavMesh.AllAreas;

    private readonly List<GameObject> _spawnedEnemies = new List<GameObject>();

    public IReadOnlyList<GameObject> SpawnedEnemies => _spawnedEnemies;
    public bool HasGenerated { get; private set; }
    public event System.Action GenerationCompleted;

    void Start()
    {
        SpawnRandom();
    }

    public void SpawnRandom()
    {
        _spawnedEnemies.Clear();

        if (_entries == null || _entries.Length == 0)
        {
            CompleteGeneration();
            return;
        }

        List<(Vector3 position, int priority)> occupied = new List<(Vector3, int)>();

        foreach (EnemyEntry entry in _entries.OrderByDescending(e => e.priority))
        {
            if (entry.prefab == null || entry.spawnPoints == null || entry.spawnPoints.Length == 0)
            {
                Debug.LogWarning($"[EnemyGen] Entrada con prefab '{entry.prefab?.name ?? "null"}' omitida: prefab o spawnPoints no configurados.", this);
                continue;
            }

            int count = Mathf.FloorToInt(entry.amount);
            float extraChance = entry.amount - count;
            if (Random.value < extraChance) count++;

            if (count == 0)
            {
                Debug.LogWarning($"[EnemyGen] '{entry.prefab.name}' tiene amount=0, no se genera nada.", this);
                continue;
            }

            List<Vector3> usedByThisEntry = new List<Vector3>();

            List<Transform> candidates = entry.spawnPoints
                .Where(p => p != null)
                .OrderBy(_ => Random.value)
                .ToList();

            int spawned = 0;

            foreach (Transform spawnPoint in candidates)
            {
                if (spawned >= count) break;

                if (usedByThisEntry.Contains(spawnPoint.position)) continue;

                if (occupied.Any(o => Vector3.Distance(o.position, spawnPoint.position) < _exclusionRadius && o.priority > entry.priority))
                    continue;

                if (!TryGetNavMeshPosition(spawnPoint.position, out Vector3 navMeshPosition))
                {
                    Debug.LogWarning($"[EnemyGen] Spawn point '{spawnPoint.name}' descartado: no hay NavMesh cerca (radio {_navMeshSampleDistance}).", spawnPoint);
                    continue;
                }

                GameObject enemy = Instantiate(entry.prefab, navMeshPosition, spawnPoint.rotation);

                if (enemy.TryGetComponent<NavMeshAgent>(out NavMeshAgent agent))
                {
                    agent.Warp(navMeshPosition);
                }
                else
                {
                    Debug.LogWarning($"[EnemyGen] '{entry.prefab.name}' no tiene NavMeshAgent.", enemy);
                }

                _spawnedEnemies.Add(enemy);
                usedByThisEntry.Add(spawnPoint.position);
                occupied.Add((navMeshPosition, entry.priority));
                spawned++;
            }

            if (spawned < count)
            {
                Debug.LogWarning($"[EnemyGen] '{entry.prefab.name}' (prioridad {entry.priority}): solo se generaron {spawned}/{count} (sin spawn points válidos en NavMesh).", this);
            }
        }

        CompleteGeneration();
    }

    private bool TryGetNavMeshPosition(Vector3 sourcePosition, out Vector3 result)
    {
        if (NavMesh.SamplePosition(sourcePosition, out NavMeshHit hit, _navMeshSampleDistance, _navMeshAreaMask))
        {
            result = hit.position;
            return true;
        }

        result = sourcePosition;
        return false;
    }

    private void CompleteGeneration()
    {
        HasGenerated = true;
        GenerationCompleted?.Invoke();
    }
}