using UnityEngine;

/// <summary>
/// Detecta cuando el barco se acerca a una isla.
/// Muestra prompt y espera que el jugador apriete E para cargar la escena.
/// </summary>
public class IslandDetector : MonoBehaviour
{
    public static bool IsTransitionAvailable { get; private set; }

    [Header("Input")]
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private NavigationRunState navigationRunState;

    private IslandSceneData _currentIsland;
    private bool _isNearIsland;

    public static event System.Action<bool, string> OnIslandProximityChanged;

    private NavigationRunState State => navigationRunState != null ? navigationRunState : NavigationRunState.Instance;

    private void OnDisable()
    {
        ClearCurrentIslandState();
    }

    private void OnDestroy()
    {
        ClearCurrentIslandState();
    }

    private void Update()
    {
        if (!_isNearIsland || _currentIsland == null)
            return;

        if (SceneLoader.Instance != null && SceneLoader.Instance.IsLoading)
            return;

        if (Input.GetKeyDown(interactKey))
            LoadCurrentIsland();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Island")) return;

        IslandSceneData islandData = other.GetComponent<IslandSceneData>();
        if (islandData == null)
        {
            Debug.LogWarning("[IslandDetector] El trigger de isla no tiene IslandSceneData.");
            return;
        }

        _currentIsland = islandData;
        _isNearIsland = true;
        IsTransitionAvailable = true;

        if (islandData.isArtifactIsland)
            State.SetCurrentArtifactIsland(islandData.zoneIndex, ResolveArtifactId(islandData));

        OnIslandProximityChanged?.Invoke(true, BuildPrompt(islandData));
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Island")) return;
        ClearCurrentIslandState();
    }

    private void LoadCurrentIsland()
    {
        if (_currentIsland == null) return;

        if (SceneLoader.Instance == null)
        {
            Debug.LogError("[IslandDetector] No SceneLoader instance found.");
            return;
        }

        if (_currentIsland.isArtifactIsland)
            State.SetCurrentArtifactIsland(_currentIsland.zoneIndex, ResolveArtifactId(_currentIsland));

        string targetSceneName = _currentIsland.sceneName;
        if (!SceneLoader.Instance.TryLoadScene(targetSceneName))
            return;

        ClearCurrentIslandState(false);
    }

    private string BuildPrompt(IslandSceneData islandData)
    {
        if (islandData == null) return string.Empty;

        string displayName = string.IsNullOrEmpty(islandData.islandName) ? "isla" : islandData.islandName;
        return $"[E] Entrar {displayName}";
    }

    private string ResolveArtifactId(IslandSceneData islandData)
    {
        if (islandData == null) return string.Empty;
        if (!string.IsNullOrWhiteSpace(islandData.artifactId)) return islandData.artifactId;

        string sceneName = string.IsNullOrWhiteSpace(islandData.sceneName.SceneName) ? "ArtifactIsland" : islandData.sceneName.SceneName;
        string objectName = string.IsNullOrWhiteSpace(islandData.gameObject.name) ? "ManualArtifact" : islandData.gameObject.name;
        string generatedId = $"manual_zone{islandData.zoneIndex}_{sceneName}_{objectName}".Replace(' ', '_');
        islandData.artifactId = generatedId;
#if UNITY_EDITOR
        Debug.Log($"[IslandDetector] Isla de artefacto manual/editor sin artifactId. Se usa {generatedId} para esta run.", islandData);
#endif
        return generatedId;
    }

    private void ClearCurrentIslandState(bool clearRunContext = true)
    {
        if (clearRunContext && _currentIsland != null && _currentIsland.isArtifactIsland)
            State.ClearCurrentArtifactIsland(_currentIsland.artifactId);

        _currentIsland = null;
        _isNearIsland = false;
        IsTransitionAvailable = false;

        OnIslandProximityChanged?.Invoke(false, string.Empty);
    }
}