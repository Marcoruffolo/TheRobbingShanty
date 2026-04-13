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

    private IslandSceneData _currentIsland;
    private bool _isNearIsland;

    // Evento para que el HUD muestre/oculte el prompt
    public static event System.Action<bool, string> OnIslandProximityChanged;

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

        string promptText = string.IsNullOrEmpty(islandData.islandName)
            ? "[E] Enter"
            : $"[E] Enter {islandData.islandName}";

        OnIslandProximityChanged?.Invoke(true, promptText);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Island")) return;

        ClearCurrentIslandState();
    }

    private void LoadCurrentIsland() //carga escena
    {
        if (_currentIsland == null) return;

        if (SceneLoader.Instance == null)
        {
            Debug.LogError("[IslandDetector] No SceneLoader instance found.");
            return;
        }

        string targetSceneName = _currentIsland.sceneName;

        if (!SceneLoader.Instance.TryLoadScene(targetSceneName))
            return;

        ClearCurrentIslandState();
    }
    private void ClearCurrentIslandState()
    {
        _currentIsland = null;
        _isNearIsland = false;
        IsTransitionAvailable = false;

        OnIslandProximityChanged?.Invoke(false, string.Empty);
    }
}
