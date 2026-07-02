using System.Collections.Generic;
using UnityEngine;

public class MonkeySpawner : MonoBehaviour
{
    [SerializeField] private EnemyMonkey monkeyPrefab;
    [SerializeField] private int minCount = 3;
    [SerializeField] private int maxCount = 8;
    [SerializeField] private ObjectProceduralGeneration[] treeGenerators;

    private bool _hasSpawned;

    private void OnEnable()
    {
        foreach (var generator in treeGenerators)
            if (generator != null) generator.GenerationCompleted += TrySpawnMonkeys;
    }

    private void OnDisable()
    {
        foreach (var generator in treeGenerators)
            if (generator != null) generator.GenerationCompleted -= TrySpawnMonkeys;
    }

    private void Start()
    {
        TrySpawnMonkeys();
    }

    private void TrySpawnMonkeys()
    {
        if (_hasSpawned || !AllGeneratorsFinished()) return;
        _hasSpawned = true;
        SpawnMonkeys();
    }

    private bool AllGeneratorsFinished()
    {
        if (treeGenerators == null || treeGenerators.Length == 0) return false;

        foreach (var generator in treeGenerators)
            if (generator == null || !generator.HasGenerated) return false;

        return true;
    }

    private void SpawnMonkeys()
    {
        if (monkeyPrefab == null)
        {
            Debug.LogWarning("[MonkeySpawner] Monkey Prefab no está asignado.");
            return;
        }

        var available = new List<TreeNodeAnchor>(TreeNodeRegistry.All);
        int count = Mathf.Min(Random.Range(minCount, maxCount + 1), available.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, available.Count);
            TreeNodeAnchor node = available[index];
            available.RemoveAt(index);

            var monkey = Instantiate(monkeyPrefab, node.transform.position, node.transform.rotation);

            var hopper = monkey.GetComponent<MonkeyTreeHopper>();
            if (hopper == null)
            {
                Debug.LogError("[MonkeySpawner] El prefab del mono no tiene MonkeyTreeHopper.");
                continue;
            }

            hopper.SetInitialNode(node);
        }
    }
}
