using UnityEngine;
using System.Collections.Generic;

public class OverworldIslandPlacer : MonoBehaviour
{
    public static OverworldIslandPlacer Instance { get; private set; }

    [Header("Configuración")]
    [SerializeField] private IslandWorldConfig config;
    [SerializeField] private GameObject overworldIslandPrefab;

    [Header("Materiales (opcional)")]
    [SerializeField] private Material proceduralIslandMaterial;
    [SerializeField] private Material manualIslandMaterial;

    private readonly List<IslandInstanceData> _allInstances = new();
    public IReadOnlyList<IslandInstanceData> AllInstances => _allInstances;

    private System.Random _rng;
    private int _resolvedSeed;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => GenerateWorld();

    public void GenerateWorld()
    {
        ClearAll();

        _resolvedSeed = config.globalSeed == 0
            ? Random.Range(1, int.MaxValue)
            : config.globalSeed;
        _rng = new System.Random(_resolvedSeed);

        PlaceManualIslands();
        PlaceProceduralIslands();

        Debug.Log($"[OverworldPlacer] {_allInstances.Count} islas generadas. Seed: {_resolvedSeed}");
    }
    private void PlaceManualIslands()
    {
        foreach (var entry in config.manualIslands)
        {
            int seed = entry.seedOverride != 0
                ? entry.seedOverride
                : Mathf.Abs((_resolvedSeed.ToString() + entry.instanceId).GetHashCode());

            float radius = GetRadiusMidpoint(entry.size);

            var inst = CreateInstanceData(
                entry.instanceId,
                entry.displayName,
                new Vector3(entry.positionXZ.x, 0f, entry.positionXZ.y),
                radius,
                seed,
                entry.size,
                isManual: true
            );

            SpawnVisual(inst, manualIslandMaterial);
            _allInstances.Add(inst);
        }

        Debug.Log($"[OverworldPlacer] {config.manualIslands.Count} islas manuales colocadas.");
    }

    private void PlaceProceduralIslands()
    {
        var occupied = BuildOccupiedList();

        int placed = 0;
        int attempts = 0;

        while (placed < config.targetIslandCount && attempts < config.maxPlacementAttempts)
        {
            attempts++;

            Vector2 candidatePos = SampleCandidatePosition();
            IslandSizeCategory size = GetWeightedRandomSize();
            IslandSizeRange range = GetSizeRange(size);
            float radius = Mathf.Lerp(
                                                  range.radiusMin,
                                                  range.radiusMax,
                                                  (float)_rng.NextDouble()
                                              );

            if (!IsPositionValid(candidatePos, radius, occupied))
                continue;

            occupied.Add((candidatePos, radius));

            int islandSeed = _rng.Next(1, int.MaxValue);
            string id = $"proc_{placed:000}";

            var inst = CreateInstanceData(
                id,
                $"Isla {placed + 1}",
                new Vector3(candidatePos.x, 0f, candidatePos.y),
                radius,
                islandSeed,
                size,
                isManual: false
            );

            SpawnVisual(inst, proceduralIslandMaterial);
            _allInstances.Add(inst);
            placed++;
        }

        Debug.Log($"[OverworldPlacer] {placed}/{config.targetIslandCount} islas procedurales " +
                  $"en {attempts} intentos.");
    }

    private Vector2 SampleCandidatePosition()
    {
        bool hasAllowedZones = config.allowedRects.Count > 0
                            || config.allowedCircles.Count > 0;

        return hasAllowedZones
            ? SampleFromAllowedZones()
            : SampleFromWholeWorld();
    }

    private Vector2 SampleFromWholeWorld()
    {
        float x = (float)_rng.NextDouble() * config.worldSize.x + config.worldOrigin.x;
        float z = (float)_rng.NextDouble() * config.worldSize.y + config.worldOrigin.y;
        return new Vector2(x, z);
    }

    private Vector2 SampleFromAllowedZones()
    {
        var zones = new List<(System.Func<Vector2> sampler, float area)>();

        foreach (var r in config.allowedRects)
            zones.Add((() => SampleRect(r.rect), r.rect.width * r.rect.height));

        foreach (var c in config.allowedCircles)
            zones.Add((() => SampleCircle(c.center, c.radius),
                       Mathf.PI * c.radius * c.radius));

        float totalArea = 0f;
        foreach (var z in zones) totalArea += z.area;

        float rand = (float)_rng.NextDouble() * totalArea;
        float cumulative = 0f;

        foreach (var (sampler, area) in zones)
        {
            cumulative += area;
            if (rand <= cumulative) return sampler();
        }

        return zones[0].sampler();
    }

    private Vector2 SampleRect(Rect r) =>
        new Vector2(
            (float)(_rng.NextDouble() * r.width + r.x),
            (float)(_rng.NextDouble() * r.height + r.y)
        );

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
        float ox = config.worldOrigin.x, oz = config.worldOrigin.y;
        if (pos.x < ox || pos.x > ox + config.worldSize.x ||
            pos.y < oz || pos.y > oz + config.worldSize.y)
            return false;

        foreach (var fr in config.forbiddenRects)
            if (fr.rect.Contains(pos)) return false;

        foreach (var fc in config.forbiddenCircles)
            if (Vector2.Distance(pos, fc.center) < fc.radius + radius) return false;

        bool hasAllowedZones = config.allowedRects.Count > 0
                            || config.allowedCircles.Count > 0;
        if (hasAllowedZones)
        {
            bool inside = false;
            foreach (var ar in config.allowedRects)
                if (ar.rect.Contains(pos)) { inside = true; break; }
            if (!inside)
                foreach (var ac in config.allowedCircles)
                    if (Vector2.Distance(pos, ac.center) <= ac.radius) { inside = true; break; }
            if (!inside) return false;
        }

        foreach (var (oPos, oRadius) in occupied)
        {
            float minDist = radius + oRadius + config.minDistanceBetweenIslands;
            if (Vector2.Distance(pos, oPos) < minDist) return false;
        }

        return true;
    }
    private List<(Vector2 pos, float radius)> BuildOccupiedList()
    {
        var list = new List<(Vector2, float)>();
        foreach (var inst in _allInstances) // en este punto solo hay manuales
            list.Add((new Vector2(inst.overworldPosition.x, inst.overworldPosition.z),
                      inst.overworldRadius));
        return list;
    }

    private IslandSizeCategory GetWeightedRandomSize()
    {
        int total = config.weightTiny + config.weightSmall + config.weightMedium
                  + config.weightLarge + config.weightHuge;
        int r = _rng.Next(total);

        if (r < config.weightTiny) return IslandSizeCategory.Tiny;
        r -= config.weightTiny;
        if (r < config.weightSmall) return IslandSizeCategory.Small;
        r -= config.weightSmall;
        if (r < config.weightMedium) return IslandSizeCategory.Medium;
        r -= config.weightMedium;
        if (r < config.weightLarge) return IslandSizeCategory.Large;
        return IslandSizeCategory.Huge;
    }

    private IslandSizeRange GetSizeRange(IslandSizeCategory cat) => cat switch
    {
        IslandSizeCategory.Tiny => config.tinyRange,
        IslandSizeCategory.Small => config.smallRange,
        IslandSizeCategory.Medium => config.mediumRange,
        IslandSizeCategory.Large => config.largeRange,
        IslandSizeCategory.Huge => config.hugeRange,
        _ => config.smallRange
    };

    private float GetRadiusMidpoint(IslandSizeCategory cat)
    {
        var r = GetSizeRange(cat);
        return (r.radiusMin + r.radiusMax) * 0.5f;
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

    private void SpawnVisual(IslandInstanceData inst, Material mat)
    {
        var go = Instantiate(overworldIslandPrefab,
                             inst.overworldPosition,
                             Quaternion.identity,
                             transform);
        go.name = $"OW_{inst.instanceId}";
        go.GetComponent<OverworldIslandVisual>().Initialize(inst, mat);
    }

    private void ClearAll()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        foreach (var inst in _allInstances)
            if (inst != null) Destroy(inst);

        _allInstances.Clear();
    }
}