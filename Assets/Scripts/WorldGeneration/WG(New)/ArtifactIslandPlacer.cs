using UnityEngine;
using System.Collections.Generic;

public class ArtifactIslandPlacer : MonoBehaviour
{
    public static ArtifactIslandPlacer Instance { get; private set; }

    [SerializeField] private WorldZoneSequence worldZoneSequence;
    [SerializeField] private BiomeIslandLibrary islandLibrary;
    [SerializeField] private IslandWorldConfig config;

    public event System.Action<IReadOnlyList<List<ArtifactIslandController>>> IslandsGenerated;

    private System.Random _rng;
    private Transform _container;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        _container = new GameObject("ArtifactIslands").transform;
        _container.SetParent(transform);
    }

    private void Start() => GenerateArtifactIslands();

    public void GenerateArtifactIslands()
    {
        ClearAll();

        if (worldZoneSequence == null)
        {
            Debug.LogError("[ArtifactIslandPlacer] worldZoneSequence no está asignado.");
            return;
        }
        if (islandLibrary == null)
        {
            Debug.LogError("[ArtifactIslandPlacer] islandLibrary no está asignado.");
            return;
        }

        int seed = worldZoneSequence.globalSeed == 0
            ? Random.Range(1, int.MaxValue)
            : worldZoneSequence.globalSeed;
        _rng = new System.Random(seed);

        var placedByZone = new List<List<ArtifactIslandController>>();
        foreach (var range in worldZoneSequence.ResolveRanges())
            placedByZone.Add(PlaceForZone(range));

        IslandsGenerated?.Invoke(placedByZone);
    }

    private List<ArtifactIslandController> PlaceForZone(ZoneRange range)
    {
        var placedControllers = new List<ArtifactIslandController>();

        var zone = range.definition;
        var prefabs = islandLibrary.GetArtifactPrefabsFor(zone.biome);
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogWarning($"[ArtifactIslandPlacer] No hay prefabs de artefacto para el bioma {zone.biome}.");
            return placedControllers;
        }

        int count = Mathf.Min(_rng.Next(zone.artifactCountMin, zone.artifactCountMax + 1), prefabs.Count);
        var shuffledPrefabs = Shuffle(prefabs);

        float zoneLength = range.endZ - range.startZ;
        float step = zoneLength / count;

        var placedPositions = new List<Vector2>();

        for (int i = 0; i < count; i++)
        {
            float targetZ = range.startZ + step * (i + 0.5f);
            Vector2 pos = ResolvePosition(targetZ, range.endZ, zone, placedPositions);
            placedPositions.Add(pos);

            var go = Instantiate(shuffledPrefabs[i], new Vector3(pos.x, 0f, pos.y), Quaternion.identity, _container);
            go.name = $"Artifact_{zone.biome}_{i}";

            var controller = go.GetComponentInChildren<ArtifactIslandController>();
            if (controller != null) placedControllers.Add(controller);
        }

        return placedControllers;
    }

    private Vector2 ResolvePosition(float startZ, float maxZ, ZoneDefinition zone, List<Vector2> placedPositions)
    {
        float z = startZ;
        float x = zone.originX + (float)_rng.NextDouble() * zone.widthX;

        const int maxPushes = 50;
        for (int pushes = 0; pushes < maxPushes &&
             (IsTooClose(new Vector2(x, z), placedPositions, zone.minArtifactDistance) || IsInForbiddenZone(new Vector2(x, z)));
             pushes++)
        {
            z += zone.minArtifactDistance * 0.5f;
            x = zone.originX + (float)_rng.NextDouble() * zone.widthX;
        }

        // Nunca pasarse del final de la zona: ahí empieza la siguiente, todavía bloqueada por su puerta.
        z = Mathf.Min(z, maxZ - 0.01f);

        return new Vector2(x, z);
    }

    private bool IsTooClose(Vector2 pos, List<Vector2> placedPositions, float minDistance)
    {
        foreach (var placed in placedPositions)
            if (Vector2.Distance(pos, placed) < minDistance) return true;
        return false;
    }

    private bool IsInForbiddenZone(Vector2 pos)
    {
        if (config == null) return false;

        foreach (var fr in config.forbiddenRects)
            if (fr.rect.Contains(pos)) return true;
        foreach (var fc in config.forbiddenCircles)
            if (Vector2.Distance(pos, fc.center) < fc.radius) return true;

        return false;
    }

    private List<GameObject> Shuffle(List<GameObject> source)
    {
        var result = new List<GameObject>(source);
        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (result[i], result[j]) = (result[j], result[i]);
        }
        return result;
    }

    private void ClearAll()
    {
        foreach (Transform child in _container) Destroy(child.gameObject);
    }

#if UNITY_EDITOR
    [UnityEditor.CustomEditor(typeof(ArtifactIslandPlacer))]
    public class ArtifactIslandPlacerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var placer = (ArtifactIslandPlacer)target;
            GUILayout.Space(10);
            if (GUILayout.Button("▶  Regenerar Islas-Artefacto", GUILayout.Height(32)))
            {
                if (Application.isPlaying) placer.GenerateArtifactIslands();
                else Debug.LogWarning("Entrá en Play Mode primero.");
            }
        }
    }
#endif
}
