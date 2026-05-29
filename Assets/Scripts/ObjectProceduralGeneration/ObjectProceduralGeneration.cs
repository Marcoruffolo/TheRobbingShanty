using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ObjectProceduralGeneration : MonoBehaviour
{
    [System.Serializable]
    private class PrefabEntry
    {
        public GameObject prefab;
        public Transform[] spawnPoints;
        public int priority;
        [Min(0f)] public float amount = 1f;
    }

    [SerializeField] private PrefabEntry[] _entries;

    void Start()
    {
        SpawnRandom();
    }

    public void SpawnRandom()
    {
        if (_entries == null || _entries.Length == 0) return;

        List<Vector3> occupied = new List<Vector3>();

        foreach (PrefabEntry entry in _entries.OrderByDescending(e => e.priority))
        {
            if (entry.prefab == null || entry.spawnPoints == null) continue;

            int count = Mathf.FloorToInt(entry.amount);
            float extraChance = entry.amount - count;
            if (Random.value < extraChance) count++;

            for (int i = 0; i < count; i++)
            {
                Transform spawnPoint = entry.spawnPoints
                    .Where(p => p != null && !occupied.Any(o => o == p.position))
                    .OrderBy(_ => Random.value)
                    .FirstOrDefault();

                if (spawnPoint == null) break;

                Instantiate(entry.prefab, spawnPoint.position, spawnPoint.rotation);
                occupied.Add(spawnPoint.position);
            }
        }
    }
}
