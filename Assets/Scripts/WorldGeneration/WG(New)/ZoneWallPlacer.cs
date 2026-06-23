using UnityEngine;
using System.Collections.Generic;

public class ZoneWallPlacer : MonoBehaviour
{
    public static ZoneWallPlacer Instance { get; private set; }

    [SerializeField] private WorldZoneSequence worldZoneSequence;
    [SerializeField] private ArtifactIslandPlacer artifactIslandPlacer;
    [SerializeField] private GameObject wallBasePrefab;
    [SerializeField] private GameObject wallDoorPrefab;

    [SerializeField] private float wallSegmentWidth = 20f;
    [SerializeField] private float doorSegmentWidth = 20f;
    [SerializeField] private float segmentOverlap = 0f;

    private readonly List<GameObject> _spawned = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        artifactIslandPlacer.IslandsGenerated += GenerateWalls;
    }

    private void OnDestroy()
    {
        if (artifactIslandPlacer != null)
            artifactIslandPlacer.IslandsGenerated -= GenerateWalls;
    }

    private void GenerateWalls(IReadOnlyList<List<ArtifactIslandController>> artifactsByZone)
    {
        ClearAll();

        var ranges = worldZoneSequence.ResolveRanges();
        if (ranges.Count == 0) return;

        PlaceStartWall(ranges[0]);

        for (int i = 0; i < ranges.Count; i++)
        {
            ZoneDefinition gateBand = i + 1 < ranges.Count ? ranges[i + 1].definition : ranges[i].definition;
            PlaceGate(ranges[i].endZ, gateBand, artifactsByZone[i]);
        }
    }

    private void PlaceStartWall(ZoneRange firstRange)
    {
        var zone = firstRange.definition;
        TileWalls(wallBasePrefab, wallSegmentWidth, zone.originX, zone.originX + zone.widthX,
                  firstRange.startZ, "ZoneWall_Start");
    }

    private void PlaceGate(float z, ZoneDefinition nextZone, List<ArtifactIslandController> zoneArtifacts)
    {
        float bandStart = nextZone.originX;
        float bandEnd = nextZone.originX + nextZone.widthX;
        float doorCenterX = bandStart + nextZone.widthX * 0.5f;
        float doorHalf = doorSegmentWidth * 0.5f;

        var doorGo = Instantiate(wallDoorPrefab, new Vector3(doorCenterX, 0f, z), Quaternion.identity, transform);
        doorGo.name = $"ZoneWall_Gate_{nextZone.biome}";
        _spawned.Add(doorGo);

        var gate = doorGo.GetComponent<ZoneGateController>();
        if (gate != null)
        {
            gate.Initialize(zoneArtifacts.Count);
            foreach (var artifact in zoneArtifacts)
                artifact.ArtifactActivated += gate.NotifyArtifactActivated;
        }

        var leftWalls = TileWalls(wallBasePrefab, wallSegmentWidth, bandStart, doorCenterX - doorHalf,
                                   z, $"ZoneWall_Gate_{nextZone.biome}_Izq");
        if (leftWalls.Count > 0)
            leftWalls[^1].transform.position += new Vector3(segmentOverlap, 0f, 0f);

        var rightWalls = TileWalls(wallBasePrefab, wallSegmentWidth, doorCenterX + doorHalf, bandEnd,
                                    z, $"ZoneWall_Gate_{nextZone.biome}_Der");
        if (rightWalls.Count > 0)
            rightWalls[0].transform.position -= new Vector3(segmentOverlap, 0f, 0f);
    }

    private List<GameObject> TileWalls(GameObject prefab, float segmentWidth, float startX, float endX, float z, string namePrefix)
    {
        var spawned = new List<GameObject>();

        float span = endX - startX;
        if (span <= 0f || segmentWidth <= 0f) return spawned;

        int count = Mathf.Max(1, Mathf.CeilToInt(span / segmentWidth));

        float pitch = Mathf.Max(0.01f, segmentWidth - segmentOverlap);

        for (int i = 0; i < count; i++)
        {
            float x = startX + segmentWidth * 0.5f + i * pitch;
            var go = Instantiate(prefab, new Vector3(x, 0f, z), Quaternion.identity, transform);
            go.name = $"{namePrefix}_{i}";
            spawned.Add(go);
            _spawned.Add(go);
        }

        return spawned;
    }

    private void ClearAll()
    {
        foreach (var go in _spawned)
            if (go != null) Destroy(go);
        _spawned.Clear();
    }
}
