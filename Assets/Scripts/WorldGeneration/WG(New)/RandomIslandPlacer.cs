using UnityEngine;
using System.Collections.Generic;

public class RandomIslandPlacer : MonoBehaviour
{
    public static RandomIslandPlacer Instance { get; private set; }

    [SerializeField] private WorldZoneSequence worldZoneSequence;
    [SerializeField] private BiomeIslandLibrary islandLibrary;
    [SerializeField] private IslandWorldConfig config;
    [SerializeField] private ArtifactIslandPlacer artifactIslandPlacer;

    [Tooltip("Radio asumido de una isla-artefacto, para no superponer islas random encima de ellas")]
    [SerializeField] private float artifactIslandRadius = 60f;

    [Tooltip("Margen de exclusión alrededor de cada muro/puerta (están en los límites de cada zona)")]
    [SerializeField] private float wallExclusionMargin = 30f;

    private Transform _container;
    private System.Random _rng;
    private IReadOnlyList<ZoneRange> _zoneRanges;
    private Dictionary<ZoneBiome, List<GameObject>> _unusedPrefabsPerBiome;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _container = new GameObject("RandomIslands").transform;
        _container.SetParent(transform);

        artifactIslandPlacer.IslandsGenerated += GenerateRandomIslands;
    }

    private void OnDestroy()
    {
        if (artifactIslandPlacer != null)
            artifactIslandPlacer.IslandsGenerated -= GenerateRandomIslands;
    }

    private void GenerateRandomIslands(IReadOnlyList<List<ArtifactIslandController>> artifactsByZone)
    {
        ClearAll();

        int seed = WorldRunSeed.Resolve(worldZoneSequence.globalSeed);
        _rng = new System.Random(seed);
        _zoneRanges = worldZoneSequence.ResolveRanges();
        _unusedPrefabsPerBiome = new Dictionary<ZoneBiome, List<GameObject>>();

        var occupied = BuildOccupiedFromArtifacts(artifactsByZone);

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
            SpawnIsland(pos, zone.biome);
            placed++;
        }
    }

    private List<(Vector2 pos, float radius)> BuildOccupiedFromArtifacts(IReadOnlyList<List<ArtifactIslandController>> artifactsByZone)
    {
        var occupied = new List<(Vector2 pos, float radius)>();
        foreach (var zoneArtifacts in artifactsByZone)
            foreach (var artifact in zoneArtifacts)
            {
                if (artifact == null) continue;
                Vector3 p = artifact.transform.position;
                occupied.Add((new Vector2(p.x, p.z), artifactIslandRadius));
            }
        return occupied;
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

    private bool IsNearWall(float z)
    {
        if (_zoneRanges.Count == 0) return false;

        if (Mathf.Abs(z - _zoneRanges[0].startZ) < wallExclusionMargin) return true;

        foreach (var r in _zoneRanges)
            if (Mathf.Abs(z - r.endZ) < wallExclusionMargin) return true;

        return false;
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
        if (IsNearWall(pos.y)) return false;

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

    private void SpawnIsland(Vector2 pos, ZoneBiome biome)
    {
        var prefabs = islandLibrary.GetRandomPrefabsFor(biome);
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning($"[RandomIslandPlacer] No hay prefabs random para el bioma {biome}.");
            return;
        }

        var prefab = PickPrefab(biome, prefabs);
        var go = Instantiate(prefab, new Vector3(pos.x, 0f, pos.y), Quaternion.identity, _container);
        go.name = $"Random_{biome}";
    }

    private GameObject PickPrefab(ZoneBiome biome, List<GameObject> allPrefabs)
    {
        if (!_unusedPrefabsPerBiome.TryGetValue(biome, out var unused))
        {
            unused = new List<GameObject>(allPrefabs);
            _unusedPrefabsPerBiome[biome] = unused;
        }

        if (unused.Count > 0)
        {
            int index = _rng.Next(unused.Count);
            var prefab = unused[index];
            unused.RemoveAt(index);
            return prefab;
        }

        return allPrefabs[_rng.Next(allPrefabs.Count)];
    }

    private void ClearAll()
    {
        foreach (Transform child in _container) Destroy(child.gameObject);
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(RandomIslandPlacer))]
    public class RandomIslandPlacerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(10);
            UnityEditor.EditorGUILayout.HelpBox(
                "Se regenera automáticamente cuando ArtifactIslandPlacer termina de generar.", UnityEditor.MessageType.Info);
        }
    }
#endif
}
