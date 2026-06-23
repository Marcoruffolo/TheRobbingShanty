using UnityEngine;
using System.Collections.Generic;

public class ZoneGateController : MonoBehaviour
{
    [SerializeField] private GameObject blockingObject;
    [SerializeField] private GameObject artifactMarkerPrefab;
    [SerializeField] private List<Transform> markerSpawnPoints;

    private int _requiredCount;
    private int _activatedCount;

    public void Initialize(int requiredCount)
    {
        _requiredCount = requiredCount;
        _activatedCount = 0;

        if (_requiredCount <= 0) Open();
    }

    public void NotifyArtifactActivated()
    {
        SpawnMarker();

        _activatedCount++;
        if (_activatedCount >= _requiredCount) Open();
    }

    private void SpawnMarker()
    {
        if (artifactMarkerPrefab == null) return;
        if (_activatedCount >= markerSpawnPoints.Count) return;

        var point = markerSpawnPoints[_activatedCount];
        Instantiate(artifactMarkerPrefab, point.position, point.rotation, point);
    }

    private void Open()
    {
        if (blockingObject != null) blockingObject.SetActive(false);
    }
}
