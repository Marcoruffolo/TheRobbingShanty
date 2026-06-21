using System.Collections.Generic;
using UnityEngine;

public class ArtifactIslandController : MonoBehaviour
{
    [SerializeField] private ObjectProceduralGeneration enemyGenerator;
    [SerializeField] private Item navigationCore;
    [SerializeField] private Collider[] coreColliders;
    [SerializeField] private GameObject lockedVisual;
    [SerializeField] private GameObject unlockedVisual;
    [SerializeField] private SOVariableBool islandCompleted;
    [SerializeField] private SOVariableBool coreClaimed;

    private readonly List<EnemyBase> _enemies = new List<EnemyBase>();
    private bool _initialized;

    private void OnEnable()
    {
        if (enemyGenerator != null)
            enemyGenerator.GenerationCompleted += OnGenerationCompleted;

        if (navigationCore != null)
            navigationCore.PickedUp += OnCorePickedUp;
    }

    private void Awake()
    {
        UpdateCoreState();
    }

    private void Start()
    {
        TryInitialize();
    }

    private void OnDisable()
    {
        if (enemyGenerator != null)
            enemyGenerator.GenerationCompleted -= OnGenerationCompleted;

        if (navigationCore != null)
            navigationCore.PickedUp -= OnCorePickedUp;

        foreach (EnemyBase enemy in _enemies)
            if (enemy != null)
                enemy.OnDeath -= OnEnemyDeath;
    }

    private void OnGenerationCompleted()
    {
        TryInitialize();
    }

    private void TryInitialize()
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

        if ((islandCompleted != null && islandCompleted.Value) || (coreClaimed != null && coreClaimed.Value))
        {
            foreach (EnemyBase enemy in _enemies)
                if (enemy != null)
                    Destroy(enemy.gameObject);

            UpdateCoreState();
            return;
        }

        foreach (EnemyBase enemy in _enemies)
            if (enemy != null && !enemy.IsDead)
                enemy.OnDeath += OnEnemyDeath;

        CheckCompletion();
    }

    private void OnEnemyDeath()
    {
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        foreach (EnemyBase enemy in _enemies)
            if (enemy != null && !enemy.IsDead)
                return;

        if (islandCompleted != null)
            islandCompleted.Value = true;

        UpdateCoreState();
    }

    private void OnCorePickedUp()
    {
        if (coreClaimed != null)
            coreClaimed.Value = true;

        if (islandCompleted != null)
            islandCompleted.Value = true;
    }

    private void UpdateCoreState()
    {
        bool claimed = coreClaimed != null && coreClaimed.Value;
        bool unlocked = islandCompleted != null && islandCompleted.Value;

        if (navigationCore != null)
            navigationCore.gameObject.SetActive(!claimed);

        if (coreColliders != null)
            foreach (Collider coreCollider in coreColliders)
                if (coreCollider != null)
                    coreCollider.enabled = unlocked && !claimed;

        if (lockedVisual != null)
            lockedVisual.SetActive(!unlocked && !claimed);

        if (unlockedVisual != null)
            unlockedVisual.SetActive(unlocked && !claimed);
    }
}
