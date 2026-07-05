using System.Collections.Generic;
using UnityEngine;

public class ArtifactIslandController : MonoBehaviour
{
    [SerializeField] private ObjectProceduralGeneration enemyGenerator;
    [SerializeField] private Item navigationCore;
    [SerializeField] private Collider[] coreColliders;
    [SerializeField] private GameObject lockedVisual;
    [SerializeField] private GameObject unlockedVisual;
    [SerializeField] private NavigationRunState navigationRunState;

    [Header("Fallback de prueba")]
    [SerializeField] private int fallbackZoneIndex;
    [SerializeField] private string fallbackArtifactId = "editor_artifact_island";

    private readonly List<EnemyBase> _enemies = new();
    private bool _initialized;
    private bool _completed;
    private string _artifactId;

    private NavigationRunState State => navigationRunState != null ? navigationRunState : NavigationRunState.Instance;

    private void OnEnable()
    {
        if (enemyGenerator != null)
            enemyGenerator.GenerationCompleted += OnGenerationCompleted;

        if (navigationCore != null)
            navigationCore.PickedUp += OnCorePickedUp;
    }

    private void Awake()
    {
        _artifactId = ResolveArtifactId();
        _completed = !string.IsNullOrWhiteSpace(_artifactId) && State.IsArtifactCompleted(_artifactId);
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

        if (string.IsNullOrWhiteSpace(_artifactId))
        {
            Debug.LogError("[ArtifactIslandController] No hay artifactId actual. El nucleo queda visible pero bloqueado para no romper la escena.", this);
            UpdateCoreState();
            return;
        }

        foreach (GameObject spawnedObject in enemyGenerator.SpawnedObjects)
        {
            if (spawnedObject == null) continue;

            EnemyBase enemy = spawnedObject.GetComponentInChildren<EnemyBase>();
            if (enemy != null)
                _enemies.Add(enemy);
        }

        if (State.IsCoreClaimed(_artifactId))
        {
            foreach (EnemyBase enemy in _enemies)
                if (enemy != null)
                    Destroy(enemy.gameObject);

            DisableReward();
            return;
        }

        if (State.IsArtifactCompleted(_artifactId))
        {
            _completed = true;
            UpdateCoreState();
            return;
        }

        foreach (EnemyBase enemy in _enemies)
            if (enemy != null && !enemy.IsDead)
                enemy.OnDeath += OnEnemyDeath;

        CheckCompletion();
    }

    private string ResolveArtifactId()
    {
        string stateId = State.CurrentArtifactIslandId;
        if (!string.IsNullOrWhiteSpace(stateId)) return stateId;

        if (!string.IsNullOrWhiteSpace(fallbackArtifactId))
        {
            State.SetCurrentArtifactIsland(fallbackZoneIndex, fallbackArtifactId);
            Debug.LogWarning($"[ArtifactIslandController] Se usa artifactId fallback '{fallbackArtifactId}' para prueba directa de escena.", this);
            return fallbackArtifactId;
        }

        return string.Empty;
    }

    private void OnEnemyDeath()
    {
        CheckCompletion();
    }

    private void CheckCompletion()
    {
        if (string.IsNullOrWhiteSpace(_artifactId) || State.IsCoreClaimed(_artifactId))
            return;

        foreach (EnemyBase enemy in _enemies)
            if (enemy != null && !enemy.IsDead)
                return;

        _completed = true;
        State.MarkArtifactCompleted(_artifactId);
        UpdateCoreState();
    }

    private void OnCorePickedUp()
    {
        if (string.IsNullOrWhiteSpace(_artifactId))
            return;

        if (State.IsCoreClaimed(_artifactId))
        {
            DisableReward();
            return;
        }

        State.MarkCoreClaimed(_artifactId);
        DisableReward();
    }

    private void UpdateCoreState()
    {
        bool hasArtifactId = !string.IsNullOrWhiteSpace(_artifactId);
        bool claimed = hasArtifactId && State.IsCoreClaimed(_artifactId);
        bool unlocked = hasArtifactId && _completed && !claimed;

        if (navigationCore != null)
            navigationCore.gameObject.SetActive(!claimed);

        if (coreColliders != null)
            foreach (Collider coreCollider in coreColliders)
                if (coreCollider != null)
                    coreCollider.enabled = unlocked;

        if (lockedVisual != null)
            lockedVisual.SetActive(!unlocked && !claimed);

        if (unlockedVisual != null)
            unlockedVisual.SetActive(unlocked);
    }

    private void DisableReward()
    {
        if (navigationCore != null)
            navigationCore.gameObject.SetActive(false);

        if (coreColliders != null)
            foreach (Collider coreCollider in coreColliders)
                if (coreCollider != null)
                    coreCollider.enabled = false;

        if (lockedVisual != null)
            lockedVisual.SetActive(false);

        if (unlockedVisual != null)
            unlockedVisual.SetActive(false);
    }
}