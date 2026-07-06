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

    [SerializeField] private int testSeedOverride = 0;

    private readonly List<IslandInstanceData> _allInstances = new();
    private readonly Dictionary<GameObject, float> _prefabRadiusCache = new();
    public IReadOnlyList<IslandInstanceData> AllInstances => _allInstances;
    public int LastResolvedSeed => _resolvedSeed;

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
        State.ResetRun();

        if (worldZoneSequence == null)
        {
            Debug.LogError("[OverworldPlacer] worldZoneSequence no esta asignado.", this);
            return;
        }

        int seedToUse = testSeedOverride != 0 ? testSeedOverride : worldZoneSequence.globalSeed;
        _resolvedSeed = State.GetOrCreateWorldSeed(seedToUse);
        _rng = new System.Random(_resolvedSeed);
        _zoneRanges = worldZoneSequence.ResolveRanges();

        InitializeNavigationZones();
        PlaceManualIslands();
        var artifactStats = PlaceArtifactIslands();
        int proceduralCount = PlaceProceduralIslands();

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

        List<GameObject> randomPrefabs = GetRandomPrefabs();

        foreach (var entry in config.manualIslands)
        {
            int seed = entry.seedOverride != 0
                ? entry.seedOverride
                : Mathf.Abs((_resolvedSeed.ToString() + entry.instanceId).GetHashCode());

            GameObject prefab = PickRandomPrefab(randomPrefabs);
            float radius = GetPrefabRadius(prefab);

            var inst = CreateInstanceData(
                entry.instanceId, entry.displayName,
                new Vector3(entry.positionXZ.x, 0f, entry.positionXZ.y),
                radius, seed, entry.size, isManual: true
            );

            SpawnIslandPrefab(prefab, inst, -1, string.Empty, false);
            _allInstances.Add(inst);
        }

        Debug.Log($"[OverworldPlacer] {config.manualIslands.Count} islas manuales.");
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
        int maxAttempts = Mathf.Max(2000, worldZoneSequence.targetIslandCount * 200);
        List<GameObject> unusedRandomPrefabs = new();

        while (placed < worldZoneSequence.targetIslandCount && attempts < maxAttempts)
        {
            attempts++;

            var candidate = SampleCandidatePosition();
            if (candidate == null) continue;
            var (pos, zone) = candidate.Value;

            GameObject prefab = PickFromPrefabBag(randomPrefabs, unusedRandomPrefabs);
            float radius = GetPrefabRadius(prefab);

            if (!IsPositionValid(pos, radius, occupied)) continue;

            occupied.Add((pos, radius));

            IslandSizeCategory size = zone.GetWeightedRandomSize(_rng);
            var inst = CreateInstanceData(
                $"proc_{placed:000}", $"Isla {placed + 1}",
                new Vector3(pos.x, 0f, pos.y),
                radius, _rng.Next(1, int.MaxValue),
                size, isManual: false
            );

            SpawnIslandPrefab(prefab, inst, -1, string.Empty, false);
            _allInstances.Add(inst);
            placed++;
        }

        if (placed < worldZoneSequence.targetIslandCount)
            Debug.LogWarning($"[OverworldPlacer] Solo {placed}/{worldZoneSequence.targetIslandCount} procedurales tras {attempts} intentos: la zona esta muy ocupada por islas manuales/artefacto para el radio real de los prefabs random.", this);
        else
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
            var grid = BuildShuffledArtifactGrid(range, zone, artifactCount);
            List<GameObject> unusedPrefabs = new();
            int generatedInZone = 0;

            for (int i = 0; i < artifactCount; i++)
            {
                GameObject prefab = PickFromPrefabBag(artifactPrefabs, unusedPrefabs);
                float radius = GetPrefabRadius(prefab);

                Vector2 pos = ResolveArtifactCellPosition(range, zone, grid, grid.cells[i], radius, occupied);

                string artifactId = $"run{_resolvedSeed}_zone{zoneIndex}_artifact{i}";
                var inst = CreateInstanceData(
                    artifactId, $"Isla del Artefacto {i + 1}",
                    new Vector3(pos.x, 0f, pos.y),
                    radius, _rng.Next(1, int.MaxValue),
                    zone.GetWeightedRandomSize(_rng), isManual: false
                );

                SpawnIslandPrefab(prefab, inst, zoneIndex, artifactId, true);
                State.RegisterArtifactIsland(zoneIndex, artifactId);
                _allInstances.Add(inst);
                occupied.Add((pos, radius));
                generated++;
                generatedInZone++;
            }

            if (generatedInZone != artifactCount)
                Debug.LogError($"[OverworldPlacer] Zona {zoneIndex} genero {generatedInZone}/{artifactCount} islas de artefacto.", this);
        }

        return (expected, generated);
    }

    private GameObject PickFromPrefabBag(List<GameObject> allPrefabs, List<GameObject> unusedPrefabs)
    {
        if (unusedPrefabs.Count == 0)
            unusedPrefabs.AddRange(allPrefabs);

        int index = _rng.Next(unusedPrefabs.Count);
        GameObject prefab = unusedPrefabs[index];
        unusedPrefabs.RemoveAt(index);
        return prefab;
    }

    private (int cols, int rows, List<Vector2Int> cells) BuildShuffledArtifactGrid(ZoneRange range, ZoneDefinition zone, int count)
    {
        float zoneLength = range.endZ - range.startZ;
        float aspect = zone.widthX / Mathf.Max(1f, zoneLength);
        int cols = Mathf.Max(1, Mathf.RoundToInt(Mathf.Sqrt(count * aspect)));
        int rows = Mathf.CeilToInt((float)count / cols);
        while (cols * rows < count) rows++;

        var cells = new List<Vector2Int>(cols * rows);
        for (int x = 0; x < cols; x++)
            for (int z = 0; z < rows; z++)
                cells.Add(new Vector2Int(x, z));

        for (int i = cells.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (cells[i], cells[j]) = (cells[j], cells[i]);
        }

        cells.RemoveRange(count, cells.Count - count);
        return (cols, rows, cells);
    }

    private Vector2 ResolveArtifactCellPosition(
        ZoneRange range,
        ZoneDefinition zone,
        (int cols, int rows, List<Vector2Int> cells) grid,
        Vector2Int cell,
        float radius,
        List<(Vector2 pos, float radius)> occupied)
    {
        float zoneLength = range.endZ - range.startZ;
        float cellWidth = zone.widthX / grid.cols;
        float cellHeight = zoneLength / grid.rows;

        float cellMinX = zone.originX + cell.x * cellWidth;
        float cellMinZ = range.startZ + cell.y * cellHeight;

        float margin = Mathf.Min(cellWidth, cellHeight) * 0.1f;
        float freeWidth = Mathf.Max(0f, cellWidth - margin * 2f);
        float freeHeight = Mathf.Max(0f, cellHeight - margin * 2f);

        const int maxCellAttempts = 30;
        for (int attempt = 0; attempt < maxCellAttempts; attempt++)
        {
            float x = cellMinX + margin + (float)_rng.NextDouble() * freeWidth;
            float z = cellMinZ + margin + (float)_rng.NextDouble() * freeHeight;
            Vector2 candidate = new(x, z);

            if (IsPositionValid(candidate, radius, occupied))
                return candidate;
        }

        float zoneMargin = Mathf.Min(radius, zone.widthX * 0.5f);
        float zoneFreeWidth = Mathf.Max(0f, zone.widthX - zoneMargin * 2f);
        float zoneFreeHeight = Mathf.Max(0f, zoneLength - zoneMargin * 2f);

        const int maxZoneAttempts = 200;
        for (int attempt = 0; attempt < maxZoneAttempts; attempt++)
        {
            float x = zone.originX + zoneMargin + (float)_rng.NextDouble() * zoneFreeWidth;
            float z = range.startZ + zoneMargin + (float)_rng.NextDouble() * zoneFreeHeight;
            Vector2 candidate = new(x, z);

            if (IsPositionValid(candidate, radius, occupied))
                return candidate;
        }

        Debug.LogError($"[OverworldPlacer] No se encontro lugar sin superposicion para el artefacto de celda ({cell.x},{cell.y}); revisar tamano de zona/radios. Se coloca igual para no perder la isla.", this);
        return new Vector2(cellMinX + cellWidth * 0.5f, cellMinZ + cellHeight * 0.5f);
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

        if (IsNearZoneGate(pos.y))
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

    private bool IsNearZoneGate(float z)
    {
        if (_zoneRanges == null) return false;

        foreach (var range in _zoneRanges)
        {
            float clearance = Mathf.Max(0f, range.definition.gateClearanceZ);
            if (clearance > 0f && Mathf.Abs(z - range.endZ) < clearance)
                return true;
        }

        return false;
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

    private float GetPrefabRadius(GameObject prefab)
    {
        if (prefab == null) return 0f;

        if (_prefabRadiusCache.TryGetValue(prefab, out float cached))
            return cached;

        float radius = ComputeFootprintRadius(prefab);
        _prefabRadiusCache[prefab] = radius;
        return radius;
    }

    private float ComputeFootprintRadius(GameObject prefab)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0)
        {
            Debug.LogWarning($"[OverworldPlacer] '{prefab.name}' no tiene Renderers; no se puede medir su tamano real.", prefab);
            return 0f;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        return new Vector2(bounds.extents.x, bounds.extents.z).magnitude;
    }

    private List<GameObject> GetArtifactPrefabs()
    {
        var result = new List<GameObject>();
        if (artifactIslandPrefabs == null) return result;

        foreach (GameObject prefab in artifactIslandPrefabs)
            if (prefab != null)
                result.Add(prefab);

        return result;
    }

    private List<GameObject> GetRandomPrefabs()
    {
        var result = new List<GameObject>();
        if (overworldIslandPrefabs == null) return result;

        foreach (GameObject prefab in overworldIslandPrefabs)
            if (prefab != null)
                result.Add(prefab);

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
    private void OnDrawGizmos()
    {
        if (worldZoneSequence == null) return;

        var ranges = worldZoneSequence.ResolveRanges();
        for (int i = 0; i < ranges.Count; i++)
        {
            var r = ranges[i];
            Gizmos.color = Color.HSVToRGB((i * 0.15f) % 1f, 0.6f, 0.9f);
            DrawZoneWireRect(r.definition.originX, r.startZ, r.definition.widthX, r.endZ - r.startZ);
            UnityEditor.Handles.Label(
                new Vector3(r.definition.originX + r.definition.widthX * 0.5f, 0f, (r.startZ + r.endZ) * 0.5f),
                $"Zona {i + 1} ({r.definition.biome})");

            float clearance = Mathf.Max(0f, r.definition.gateClearanceZ);
            if (clearance > 0f)
            {
                Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.6f);
                DrawZoneWireRect(r.definition.originX, r.endZ - clearance, r.definition.widthX, clearance);
            }
        }
    }

    private void DrawZoneWireRect(float originX, float originZ, float width, float depth)
    {
        Vector3 a = new(originX, 0f, originZ);
        Vector3 b = new(originX + width, 0f, originZ);
        Vector3 c = new(originX + width, 0f, originZ + depth);
        Vector3 d = new(originX, 0f, originZ + depth);
        Gizmos.DrawLine(a, b); Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d); Gizmos.DrawLine(d, a);
    }

    [UnityEditor.CustomEditor(typeof(OverworldIslandPlacer))]
    public class OverworldIslandPlacerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var placer = (OverworldIslandPlacer)target;
            GUILayout.Space(10);

            if (Application.isPlaying)
                GUILayout.Label($"Ultima seed usada: {placer.LastResolvedSeed}");

            if (GUILayout.Button("Regenerar Mundo", GUILayout.Height(32)))
            {
                if (Application.isPlaying) placer.GenerateWorld();
                else Debug.LogWarning("Entra en Play Mode primero.");
            }
        }
    }
#endif
}
