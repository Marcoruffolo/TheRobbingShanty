using UnityEngine;

public class ZoneGateController : MonoBehaviour
{
    [SerializeField] private NavigationRunState navigationRunState;
    [SerializeField] private int zoneIndex;
    [SerializeField] private GameObject blockingObject;
    [SerializeField] private WifiSpawner wifiSpawner;

    private int _requiredCount;
    private bool _missingSpawnerLogged;

    private NavigationRunState State => navigationRunState != null ? navigationRunState : NavigationRunState.Instance;

    private void Awake()
    {
        if (wifiSpawner == null)
            wifiSpawner = GetComponent<WifiSpawner>();
    }

    private void OnEnable()
    {
        NavigationRunState state = State;
        if (state != null)
            state.ZoneProgressChanged += HandleZoneProgressChanged;
    }

    private void Start()
    {
        NavigationRunState state = State;
        if (state != null && state.IsZoneReady(zoneIndex))
            Initialize(zoneIndex, state.GetRequiredCores(zoneIndex));
        else
            Refresh(0, 0);
    }

    private void OnDisable()
    {
        NavigationRunState state = State;
        if (state != null)
            state.ZoneProgressChanged -= HandleZoneProgressChanged;
    }

    public void Initialize(int zoneIndex, int requiredCount)
    {
        this.zoneIndex = zoneIndex;
        _requiredCount = Mathf.Max(0, requiredCount);
        BuildSticks(_requiredCount);

        NavigationRunState state = State;
        int placedCores = state != null ? state.GetPlacedCores(zoneIndex) : 0;
        Refresh(placedCores, _requiredCount);
    }

    public void Initialize(int requiredCount)
    {
        Initialize(zoneIndex, requiredCount);
    }

    private void HandleZoneProgressChanged(int changedZoneIndex, int placedCores, int requiredCores)
    {
        if (changedZoneIndex != zoneIndex) return;

        if (_requiredCount != requiredCores)
        {
            _requiredCount = Mathf.Max(0, requiredCores);
            BuildSticks(_requiredCount);
        }

        Refresh(placedCores, requiredCores);
    }

    private void BuildSticks(int requiredCount)
    {
        WifiSpawner spawner = ResolveSpawner();
        if (spawner == null)
        {
            LogMissingSpawner();
            return;
        }

        spawner.BuildSticks(requiredCount);
    }

    private void Refresh(int placedCores, int requiredCores)
    {
        int required = Mathf.Max(0, requiredCores);
        int placed = Mathf.Clamp(placedCores, 0, required);

        WifiSpawner spawner = ResolveSpawner();
        if (spawner != null)
            spawner.SetLitCount(placed);
        else if (required > 0)
            LogMissingSpawner();

        if (required > 0 && placed >= required)
            Open();
        else
            Close();
    }

    private WifiSpawner ResolveSpawner()
    {
        if (wifiSpawner == null)
            wifiSpawner = GetComponent<WifiSpawner>();

        return wifiSpawner;
    }

    private void LogMissingSpawner()
    {
        if (_missingSpawnerLogged) return;

        Debug.LogError("[ZoneGateController] Falta WifiSpawner para mostrar los sticks de la puerta.", this);
        _missingSpawnerLogged = true;
    }

    private void Open()
    {
        if (blockingObject != null) blockingObject.SetActive(false);
    }

    private void Close()
    {
        if (blockingObject != null) blockingObject.SetActive(true);
    }
}