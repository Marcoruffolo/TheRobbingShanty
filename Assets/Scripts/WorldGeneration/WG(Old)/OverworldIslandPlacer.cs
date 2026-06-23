using UnityEngine;
using System.Collections.Generic;

public class OverworldIslandPlacer : MonoBehaviour
{
    public static OverworldIslandPlacer Instance { get; private set; }

    [Header("Config estructural")]
    [SerializeField] private IslandWorldConfig config;
    [SerializeField] private WorldZoneSequence worldZoneSequence;
    [SerializeField] private List<GameObject> overworldIslandPrefabs;

    private readonly List<IslandInstanceData> _allInstances = new();
    public IReadOnlyList<IslandInstanceData> AllInstances => _allInstances;

    private System.Random _rng;
    private int _resolvedSeed;
    private IReadOnlyList<ZoneRange> _zoneRanges;


    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => GenerateWorld();


    public void GenerateWorld()
    {
        ClearAll();

        _resolvedSeed = worldZoneSequence.globalSeed == 0
            ? Random.Range(1, int.MaxValue)
            : worldZoneSequence.globalSeed;
        _rng = new System.Random(_resolvedSeed);
        _zoneRanges = worldZoneSequence.ResolveRanges();

        PlaceManualIslands();
        PlaceProceduralIslands();

        Debug.Log($"[OverworldPlacer] {_allInstances.Count} islas. Seed: {_resolvedSeed}");
    }

    private void PlaceManualIslands()
    {
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

            SpawnVisual(inst);
            _allInstances.Add(inst);
        }

        Debug.Log($"[OverworldPlacer] {config.manualIslands.Count} islas manuales.");
    }

    private void PlaceProceduralIslands()
    {
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
            float radius = Mathf.Lerp(range.radiusMin, range.radiusMax,
                                                   (float)_rng.NextDouble());

            if (!IsPositionValid(pos, radius, occupied)) continue;

            occupied.Add((pos, radius));

            var inst = CreateInstanceData(
                $"proc_{placed:000}", $"Isla {placed + 1}",
                new Vector3(pos.x, 0f, pos.y),
                radius, _rng.Next(1, int.MaxValue),
                size, isManual: false
            );

            SpawnVisual(inst);
            _allInstances.Add(inst);
            placed++;
        }

        Debug.Log($"[OverworldPlacer] {placed}/{worldZoneSequence.targetIslandCount} procedurales " +
                  $"en {attempts} intentos.");
    }

    private (Vector2 pos, ZoneDefinition zone)? SampleCandidatePosition()
    {
        bool hasAllowed = config.allowedRects.Count > 0 || config.allowedCircles.Count > 0;
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

    private bool IsPositionValid(Vector2 pos, float radius,
                                  List<(Vector2 pos, float radius)> occupied)
    {
        // 1. Fuera de zonas prohibidas
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

        // 2. Separación mínima entre islas
        foreach (var (oPos, oRadius) in occupied)
            if (Vector2.Distance(pos, oPos) < radius + oRadius + worldZoneSequence.minIslandDistance)
                return false;

        return true;
    }

    private List<(Vector2 pos, float radius)> BuildOccupiedList()
    {
        var list = new List<(Vector2, float)>();
        foreach (var inst in _allInstances)
            list.Add((new Vector2(inst.overworldPosition.x, inst.overworldPosition.z),
                      inst.overworldRadius));
        return list;
    }

    private IslandInstanceData CreateInstanceData(
        string id, string name, Vector3 worldPos, float radius,
        int seed, IslandSizeCategory size, bool isManual)
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

    private void SpawnVisual(IslandInstanceData inst)
    {
        if (overworldIslandPrefabs == null || overworldIslandPrefabs.Count == 0)
        {
            Debug.LogError("[OverworldPlacer] No hay prefabs de isla asignados.");
            return;
        }

        var prefab = overworldIslandPrefabs[_rng.Next(overworldIslandPrefabs.Count)];
        var go = Instantiate(prefab, inst.overworldPosition, Quaternion.identity, transform);
        go.name = $"OW_{inst.instanceId}";
        go.GetComponent<OverworldIslandVisual>()?.Initialize(inst);
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
            if (GUILayout.Button("▶  Regenerar Mundo", GUILayout.Height(32)))
            {
                if (Application.isPlaying) placer.GenerateWorld();
                else Debug.LogWarning("Entrá en Play Mode primero.");
            }
        }
    }
#endif
}
