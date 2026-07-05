using System.Collections.Generic;
using UnityEngine;

public class ZoneGateController : MonoBehaviour
{
    [SerializeField] private NavigationRunState navigationRunState;
    [SerializeField] private int zoneIndex;
    [SerializeField] private GameObject blockingObject;
    [SerializeField] private GameObject artifactMarkerPrefab;
    [SerializeField] private Transform markerRoot;
    [SerializeField] private Vector3 markerLocalOffset = new(0f, 0.12f, -0.06f);
    [SerializeField] private Vector2 markerSpacing = new(0.18f, 0.22f);
    [SerializeField] private float markerScale = 0.14f;
    [SerializeField] private int markersPerRow = 4;
    [SerializeField] private Color inactiveMarkerColor = new(0.04f, 0.05f, 0.08f, 1f);
    [SerializeField] private Color activeMarkerColor = new(0.05f, 0.9f, 1f, 1f);

    private readonly List<GameObject> _spawnedMarkers = new();
    private int _requiredCount;

    private NavigationRunState State => navigationRunState != null ? navigationRunState : NavigationRunState.Instance;

    private void OnEnable()
    {
        State.ZoneProgressChanged += HandleZoneProgressChanged;
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
        BuildMarkers(_requiredCount);
        Refresh(State.GetPlacedCores(zoneIndex), _requiredCount);
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
            BuildMarkers(_requiredCount);
        }

        Refresh(placedCores, requiredCores);
    }

    private void BuildMarkers(int requiredCount)
    {
        foreach (GameObject marker in _spawnedMarkers)
            if (marker != null)
                Destroy(marker);
        _spawnedMarkers.Clear();

        if (requiredCount <= 0) return;

        if (artifactMarkerPrefab == null)
        {
            Debug.LogError("[ZoneGateController] Falta artifactMarkerPrefab para mostrar nucleos de muralla.", this);
            return;
        }

        Transform root = markerRoot != null ? markerRoot : transform;
        int columns = Mathf.Clamp(markersPerRow, 1, requiredCount);
        int rows = Mathf.CeilToInt(requiredCount / (float)columns);

        for (int i = 0; i < requiredCount; i++)
        {
            GameObject marker = Instantiate(artifactMarkerPrefab, root);
            marker.name = $"GateCoreMarker_{i + 1}";
            marker.transform.localPosition = GetMarkerLocalPosition(i, columns, rows);
            marker.transform.localRotation = Quaternion.identity;
            marker.transform.localScale = Vector3.one * Mathf.Max(0.01f, markerScale);
            _spawnedMarkers.Add(marker);
        }
    }

    private Vector3 GetMarkerLocalPosition(int index, int columns, int rows)
    {
        int column = index % columns;
        int row = index / columns;
        float x = (column - (columns - 1) * 0.5f) * markerSpacing.x;
        float z = (row - (rows - 1) * 0.5f) * markerSpacing.y;
        return markerLocalOffset + new Vector3(x, 0f, z);
    }

    private void Refresh(int placedCores, int requiredCores)
    {
        int required = Mathf.Max(0, requiredCores);
        int placed = Mathf.Clamp(placedCores, 0, required);

        for (int i = 0; i < _spawnedMarkers.Count; i++)
        {
            GameObject marker = _spawnedMarkers[i];
            if (marker == null) continue;

            bool visible = i < required;
            marker.SetActive(visible);
            if (visible)
                TintMarker(marker, i < placed ? activeMarkerColor : inactiveMarkerColor);
        }

        if (required > 0 && placed >= required)
            Open();
        else
            Close();
    }

    private void Open()
    {
        if (blockingObject != null) blockingObject.SetActive(false);
    }

    private void Close()
    {
        if (blockingObject != null) blockingObject.SetActive(true);
    }

    private void TintMarker(GameObject marker, Color color)
    {
        Renderer[] renderers = marker.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer markerRenderer in renderers)
        {
            if (markerRenderer == null) continue;
            foreach (Material material in markerRenderer.materials)
                if (material != null && material.HasProperty("_Color"))
                    material.color = color;
        }
    }
}