using UnityEngine;
using System.Collections.Generic;

public class OverworldIslandPlacer : MonoBehaviour
{
    public static OverworldIslandPlacer Instance { get; private set; }

    [Header("Config estructural")]
    [SerializeField] private IslandWorldConfig config;
    [SerializeField] private WorldZoneSequence worldZoneSequence;
    [SerializeField] private NavigationRunState navigationRunState;
    [SerializeField] private List<GameObject> overworldIslandPrefabs;
    [SerializeField] private List<GameObject> artifactIslandPrefabs;

    [Header("Zona segura inicial")]
    [SerializeField] private Vector2 startExclusionCenter = Vector2.zero;
    [SerializeField] private float startExclusionRadius = 80f;

    private readonly List<IslandInstanceData> _allInstances = new();
    public IReadOnlyList<IslandInstanceData> AllInstances => _allInstances;

    private System.Random _rng;
    private int _resolvedSeed;
    private IReadOnlyList<ZoneRange> _zoneRanges;
    private NavigationRunState State => navigationRunState != null ? navigationRunState : NavigationRunState.Instance;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => GenerateWorld();

    public void GenerateWorld()
    {
        ClearAll();
        State.ClearCurrentIslandContext();

        if (worldZoneSequence == null)
        {
            Debug.LogError("[OverworldPlacer] worldZoneSequence no esta asignado.", this);
            return;
        }

        _resolvedSeed = State.GetOrCreateWorldSeed(worldZoneSequence.globalSeed);
        _rng = new System.Random(_resolvedSeed);
        _zoneRanges = worldZoneSequence.ResolveRanges();

        InitializeNavigationZones();
        PlaceManualIslands();
        int proceduralCount = PlaceProceduralIslands();
        var artifactStats = PlaceArtifactIslands();

        Debug.Log($"[OverworldPlacer] {_allInstances.Count} islas. Procedurales normales: {proceduralCount}/{worldZoneSequence.targetIslandCount}. Artefactos: {artifactStats.generated}/{artifactStats.expected}. Seed: {_resolvedSeed}");
    }

    private void InitializeNavigationZones()
    {
        System.Random zoneRng = new(_resolvedSeed ^ 0x4E415653);

        for (int zoneIndex = 0; zoneIndex < _zoneRanges.Count; zoneIndex++)
        {
            ZoneDefinition zone = _zoneRanges[zoneIndex].definition;
            int requiredCores = zone.RollRequiredCores(zoneRng);
            int availableCoreIslands = requiredCores + zone.RollExtraCoreIslands(zoneRng);
            State.StartZone(zoneIndex, requiredCores, availableCoreIslands);
        }
    }

    private void PlaceManualIslands()
    {
        if (config == null) return;

        foreach (var entry in config.manualIslands)
        {
            int seed = entry.seedOverride != 0
                ? entry.seedOverride
                : Mathf.Abs((_resolvedSeed.ToString() + entry.instanceId).GetHashCode());

            var range = worldZoneSequence.GetSizeRange(entry.size);
            float radius = (range.radiusMin + range.radiusMax) * 0.5f;

            var inst = CreateInstanceData(
                entry.instanceId, entry.displayName,
                new Vector3(entry.positionXZ.x, 0f, entry.positionXZ.y),
                radius, seed, entry.size, isManual: true
            );

            SpawnIslandPrefab(PickRandomPrefab(GetRandomPrefabs()), inst, -1, string.Empty, false);
            _allInstances.Add(inst);
        }

        Debug.Log($"[OverworldPlacer] {(config != null ? config.manualIslands.Count : 0)} islas manuales.");
    }

    private int PlaceProceduralIslands()
    {
        List<GameObject> randomPrefabs = GetRandomPrefabs();
        if (randomPrefabs.Count == 0)
        {
            Debug.LogWarning("[OverworldPlacer] No hay prefabs random; se omite generacion random.", this);
            return 0;
        }

        var occupied = BuildOccupiedList();
        int placed = 0;
        int attempts = 0;
        int maxAttempts = worldZoneSequence.targetIslandCount * 25;

        while (placed < worldZoneSequence.targetIslandCount && attempts < maxAttempts)
        {
            attempts++;

            var candidate = SampleCandidatePosition();
            if (candidate == null) continue;
            var (pos, zone) = candidate.Value;

            IslandSizeCategory size = zone.GetWeightedRandomSize(_rng);
            var range = worldZoneSequence.GetSizeRange(size);
            float radius = Mathf.Lerp(range.radiusMin, range.radiusMax, (float)_rng.NextDouble());

            if (!IsPositionValid(pos, radius, occupied)) continue;

            occupied.Add((pos, radius));

            var inst = CreateInstanceData(
                $"proc_{placed:000}", $"Isla {placed + 1}",
                new Vector3(pos.x, 0f, pos.y),
                radius, _rng.Next(1, int.MaxValue),
                size, isManual: false
            );

            SpawnIslandPrefab(PickRandomPrefab(randomPrefabs), inst, -1, string.Empty, false);
            _allInstances.Add(inst);
            placed++;
        }

        Debug.Log($"[OverworldPlacer] {placed}/{worldZoneSequence.targetIslandCount} procedurales en {attempts} intentos.");
        return placed;
    }

    private (int expected, int generated) PlaceArtifactIslands()
    {
        int expected = GetExpectedArtifactIslandCount();
        int generated = 0;
        List<GameObject> artifactPrefabs = GetArtifactPrefabs();
        if (artifactPrefabs.Count == 0)
        {
            Debug.LogError("[OverworldPlacer] No hay prefabs de isla de artefacto en artifactIslandPrefabs.", this);
            return (expected, generated);
        }

        for (int zoneIndex = 0; zoneIndex < _zoneRanges.Count; zoneIndex++)
        {
            ZoneRange range = _zoneRanges[zoneIndex];
            ZoneDefinition zone = range.definition;
            int artifactCount = State.GetAvailableCoreIslandCount(zoneIndex);
            if (artifactCount <= 0)
            {
                Debug.LogError($"[OverworldPlacer] Zona {zoneIndex} no inicializo islas de artefacto.", this);
                continue;
            }

            List<(Vector2 pos, float radius)> occupied = BuildOccupiedList();
            List<Vector2> placedArtifactPositions = new();
            int generatedInZone = 0;

            for (int i = 0; i < artifactCount; i++)
            {
                IslandSizeCategory size = zone.GetWeightedRandomSize(_rng);
                IslandSizeRange sizeRange = worldZoneSequence.GetSizeRange(size);
                float radius = Mathf.Lerp(sizeRange.radiusMin, sizeRange.radiusMax, (float)_rng.NextDouble());

                if (!TryResolveArtifactPosition(range, zone, radius, i, artifactCount, occupied, placedArtifactPositions, out Vector2 pos))
                {
                    Debug.LogError($"[OverworldPlacer] No se encontro posicion valida para artefacto {i + 1}/{artifactCount} en zona {zoneIndex}.", this);
                    continue;
                }

                string artifactId = $"run{_resolvedSeed}_zone{zoneIndex}_artifact{i}";
                var inst = CreateInstanceData(
                    artifactId, $"Isla del Artefacto {i + 1}",
                    new Vector3(pos.x, 0f, pos.y),
                    radius, _rng.Next(1, int.MaxValue),
                    size, isManual: false
                );

                SpawnIslandPrefab(PickRandomPrefab(artifactPrefabs), inst, zoneIndex, artifactId, true);
                State.RegisterArtifactIsland(zoneIndex, artifactId);
                _allInstances.Add(inst);
                occupied.Add((pos, radius));
                placedArtifactPositions.Add(pos);
                generated++;
                generatedInZone++;
            }

            if (generatedInZone != artifactCount)
                Debug.LogError($"[OverworldPlacer] Zona {zoneIndex} genero {generatedInZone}/{artifactCount} islas de artefacto.", this);
        }

        return (expected, generated);
    }

    private bool TryResolveArtifactPosition(
        ZoneRange range,
        ZoneDefinition zone,
        float radius,
        int artifactIndex,
        int artifactCount,
        List<(Vector2 pos, float radius)> occupied,
        List<Vector2> placedArtifacts,
        out Vector2 result)
    {
        float zoneLength = range.endZ - range.startZ;
        float step = zoneLength / Mathf.Max(1, artifactCount);
        float targetZ = range.startZ + step * (artifactIndex + 0.5f);
        float spread = Mathf.Max(step * 0.45f, zone.minArtifactDistance);
        int maxAttempts = Mathf.Max(500, artifactCount * 150);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            bool targeted = attempt < maxAttempts * 0.7f;
            float x = zone.originX + (float)_rng.NextDouble() * zone.widthX;
            float z = targeted
                ? Mathf.Clamp(targetZ + ((float)_rng.NextDouble() * 2f - 1f) * spread, range.startZ, range.endZ)
                : range.startZ + (float)_rng.NextDouble() * zoneLength;

            Vector2 candidate = new(x, z);
            float minArtifactDistance = Mathf.Max(zone.minArtifactDistance, worldZoneSequence.minIslandDistance);
            if (!IsArtifactTooClose(candidate, placedArtifacts, minArtifactDistance) && IsPositionValid(candidate, radius, occupied))
            {
                result = candidate;
                return true;
            }
        }

        result = default;
        return false;
    }

    private bool IsArtifactTooClose(Vector2 pos, List<Vector2> placedPositions, float minDistance)
    {
        foreach (var placed in placedPositions)
            if (Vector2.Distance(pos, placed) < minDistance) return true;
        return false;
    }

    private (Vector2 pos, ZoneDefinition zone)? SampleCandidatePosition()
    {
        bool hasAllowed = config != null && (config.allowedRects.Count > 0 || config.allowedCircles.Count > 0);
        if (hasAllowed)
        {
            Vector2 pos = SampleFromAllowedZones();
            ZoneDefinition zone = FindZoneAt(pos.y);
            return zone != null ? (pos, zone) : null;
        }
        return SampleFromZoneBands();
    }

    private (Vector2 pos, ZoneDefinition zone)? SampleFromZoneBands()
    {
        if (_zoneRanges.Count == 0) return null;

        float totalArea = 0f;
        foreach (var r in _zoneRanges) totalArea += (r.endZ - r.startZ) * r.definition.widthX;

        float rand = (float)_rng.NextDouble() * totalArea;
        float cumulative = 0f;
        foreach (var r in _zoneRanges)
        {
            cumulative += (r.endZ - r.startZ) * r.definition.widthX;
            if (rand > cumulative) continue;

            float x = (float)(_rng.NextDouble() * r.definition.widthX + r.definition.originX);
            float z = (float)(_rng.NextDouble() * (r.endZ - r.startZ) + r.startZ);
            return (new Vector2(x, z), r.definition);
        }

        var last = _zoneRanges[^1];
        return (new Vector2(last.definition.originX, last.startZ), last.definition);
    }

    private ZoneDefinition FindZoneAt(float z)
    {
        foreach (var r in _zoneRanges)
            if (r.Contains(z)) return r.definition;
        return null;
    }

    private Vector2 SampleFromAllowedZones()
    {
        var zones = new List<(System.Func<Vector2> sampler, float area)>();

        foreach (var r in config.allowedRects)
            zones.Add((() => SampleRect(r.rect), r.rect.width * r.rect.height));
        foreach (var c in config.allowedCircles)
            zones.Add((() => SampleCircle(c.center, c.radius), Mathf.PI * c.radius * c.radius));

        float total = 0f;
        foreach (var z in zones) total += z.area;

        float rand = (float)_rng.NextDouble() * total;
        float cumulative = 0f;
        foreach (var (sampler, area) in zones)
        {
            cumulative += area;
            if (rand <= cumulative) return sampler();
        }
        return zones[0].sampler();
    }

    private Vector2 SampleRect(Rect r) =>
        new((float)(_rng.NextDouble() * r.width + r.x),
            (float)(_rng.NextDouble() * r.height + r.y));

    private Vector2 SampleCircle(Vector2 center, float radius)
    {
        for (int i = 0; i < 50; i++)
        {
            float x = (float)(_rng.NextDouble() * 2 - 1) * radius + center.x;
            float y = (float)(_rng.NextDouble() * 2 - 1) * radius + center.y;
            if (Vector2.Distance(new Vector2(x, y), center) <= radius)
                return new Vector2(x, y);
        }
        return center;
    }

    private bool IsPositionValid(Vector2 pos, float radius, List<(Vector2 pos, float radius)> occupied)
    {
        if (startExclusionRadius > 0f && Vector2.Distance(pos, startExclusionCenter) < startExclusionRadius + radius)
            return false;

        if (config != null)
        {
            foreach (var fr in config.forbiddenRects)
                if (fr.rect.Contains(pos)) return false;
            foreach (var fc in config.forbiddenCircles)
                if (Vector2.Distance(pos, fc.center) < fc.radius + radius) return false;

            bool hasAllowed = config.allowedRects.Count > 0 || config.allowedCircles.Count > 0;
            if (hasAllowed)
            {
                bool inside = false;
                foreach (var ar in config.allowedRects)
                    if (ar.rect.Contains(pos)) { inside = true; break; }
                if (!inside)
                    foreach (var ac in config.allowedCircles)
                        if (Vector2.Distance(pos, ac.center) <= ac.radius) { inside = true; break; }
                if (!inside) return false;
            }
        }

        foreach (var (oPos, oRadius) in occupied)
            if (Vector2.Distance(pos, oPos) < radius + oRadius + worldZoneSequence.minIslandDistance)
                return false;

        return true;
    }

    private List<(Vector2 pos, float radius)> BuildOccupiedList()
    {
        var list = new List<(Vector2, float)>();
        foreach (var inst in _allInstances)
            list.Add((new Vector2(inst.overworldPosition.x, inst.overworldPosition.z), inst.overworldRadius));
        return list;
    }

    private IslandInstanceData CreateInstanceData(string id, string name, Vector3 worldPos, float radius, int seed, IslandSizeCategory size, bool isManual)
    {
        var inst = ScriptableObject.CreateInstance<IslandInstanceData>();
        inst.instanceId = id;
        inst.displayName = name;
        inst.overworldPosition = worldPos;
        inst.overworldRadius = radius;
        inst.seed = seed;
        inst.sizeCategory = size;
        inst.isManuallyPlaced = isManual;
        return inst;
    }

    private void SpawnIslandPrefab(GameObject prefab, IslandInstanceData inst, int zoneIndex, string artifactId, bool isArtifact)
    {
        if (prefab == null)
        {
            Debug.LogError("[OverworldPlacer] Prefab de isla nulo.", this);
            return;
        }

        var go = Instantiate(prefab, inst.overworldPosition, Quaternion.identity, transform);
        go.name = isArtifact ? $"OW_Artifact_{artifactId}" : $"OW_{inst.instanceId}";
        go.GetComponent<OverworldIslandVisual>()?.Initialize(inst);

        IslandSceneData sceneData = go.GetComponentInChildren<IslandSceneData>();
        if (sceneData != null)
        {
            sceneData.isArtifactIsland = isArtifact;
            sceneData.zoneIndex = zoneIndex;
            sceneData.artifactId = isArtifact ? artifactId : string.Empty;
        }
    }

    private GameObject PickRandomPrefab(List<GameObject> prefabs)
    {
        if (prefabs == null || prefabs.Count == 0) return null;
        return prefabs[_rng.Next(prefabs.Count)];
    }

    private List<GameObject> GetArtifactPrefabs()
    {
        var result = new List<GameObject>();
        if (artifactIslandPrefabs == null) return result;

        foreach (GameObject prefab in artifactIslandPrefabs)
        {
            if (prefab == null) continue;
            IslandSceneData sceneData = prefab.GetComponentInChildren<IslandSceneData>();
            if (sceneData != null && (sceneData.isArtifactIsland || sceneData.sceneName.SceneName == "ArtifactIsland"))
                result.Add(prefab);
        }
        return result;
    }

    private List<GameObject> GetRandomPrefabs()
    {
        var result = new List<GameObject>();
        if (overworldIslandPrefabs == null) return result;

        foreach (GameObject prefab in overworldIslandPrefabs)
        {
            if (prefab == null) continue;
            IslandSceneData sceneData = prefab.GetComponentInChildren<IslandSceneData>();
            bool isArtifact = sceneData != null && (sceneData.isArtifactIsland || sceneData.sceneName.SceneName == "ArtifactIsland");
            if (!isArtifact)
                result.Add(prefab);
        }
        return result;
    }

    private int GetExpectedArtifactIslandCount()
    {
        int total = 0;
        if (_zoneRanges == null) return total;

        for (int zoneIndex = 0; zoneIndex < _zoneRanges.Count; zoneIndex++)
            total += State.GetAvailableCoreIslandCount(zoneIndex);

        return total;
    }

    private void ClearAll()
    {
        foreach (Transform child in transform) Destroy(child.gameObject);
        foreach (var inst in _allInstances) if (inst) Destroy(inst);
        _allInstances.Clear();
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(OverworldIslandPlacer))]
    public class OverworldIslandPlacerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var placer = (OverworldIslandPlacer)target;
            GUILayout.Space(10);
            if (GUILayout.Button("Regenerar Mundo", GUILayout.Height(32)))
            {
                if (Application.isPlaying) placer.GenerateWorld();
                else Debug.LogWarning("Entra en Play Mode primero.");
            }
        }
    }
#endif
}