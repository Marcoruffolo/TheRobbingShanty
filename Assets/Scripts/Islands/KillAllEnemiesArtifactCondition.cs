using System.Collections.Generic;
using UnityEngine;

public class KillAllEnemiesArtifactCondition : MonoBehaviour, IArtifactUnlockCondition
{
    [SerializeField] private ObjectProceduralGeneration enemyGenerator;
    [SerializeField] private string prompt = "Nucleo bloqueado. Elimina a los enemigos para liberarlo.";

    private readonly List<EnemyBase> _enemies = new();
    private bool _initialized;

    public bool IsComplete
    {
        get
        {
            if (!_initialized) return false;
            foreach (EnemyBase enemy in _enemies)
                if (enemy != null && !enemy.IsDead)
                    return false;
            return true;
        }
    }

    public string Prompt => prompt;

    private void OnEnable()
    {
        if (enemyGenerator != null)
            enemyGenerator.GenerationCompleted += Initialize;
    }

    private void Start()
    {
        Initialize();
    }

    private void OnDisable()
    {
        if (enemyGenerator != null)
            enemyGenerator.GenerationCompleted -= Initialize;
    }

    private void Initialize()
    {
        if (_initialized || enemyGenerator == null || !enemyGenerator.HasGenerated) return;

        _initialized = true;
        foreach (GameObject spawnedObject in enemyGenerator.SpawnedObjects)
        {
            if (spawnedObject == null) continue;
            EnemyBase enemy = spawnedObject.GetComponentInChildren<EnemyBase>();
            if (enemy != null)
                _enemies.Add(enemy);
        }
    }
}
