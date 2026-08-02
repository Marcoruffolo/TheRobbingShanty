using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WorldZoneSequence", menuName = "TRS/World Zone Sequence")]
public class WorldZoneSequence : ScriptableObject
{
    [Header("Zonas")]
    [Tooltip("Orden de avance (Z) de las zonas, de inicio a fin del mundo")]
    public List<ZoneDefinition> zones = new();

    [Header("Debug")]
    [SerializeField] private NavigationRunState navigationRunState;

    [Header("Mundo")]
    public int globalSeed = 0;
    public int targetIslandCount = 20;
    public float minIslandDistance = 50f;
    public float worldOriginZ = 0f;

    [Header("Radios por categoría (islas random, únicos para todo el mundo)")]
    public float radiusSmallMin = 20f;
    public float radiusSmallMax = 40f;
    public float radiusMediumMin = 40f;
    public float radiusMediumMax = 80f;
    public float radiusLargeMin = 80f;
    public float radiusLargeMax = 150f;

    public float TotalDepthZ
    {
        get
        {
            float total = 0f;
            foreach (var zone in zones) total += zone.lengthZ;
            return total;
        }
    }

    public IReadOnlyList<ZoneRange> ResolveRanges()
    {
        var ranges = new List<ZoneRange>(zones.Count);
        float cursor = worldOriginZ;
        foreach (var zone in zones)
        {
            ranges.Add(new ZoneRange(zone, cursor, cursor + zone.lengthZ));
            cursor += zone.lengthZ;
        }
        return ranges;
    }

#if UNITY_EDITOR
    private void OnEnable()
    {
        UnityEditor.EditorApplication.update += ProcessDebugAutoComplete;
    }

    private void OnDisable()
    {
        UnityEditor.EditorApplication.update -= ProcessDebugAutoComplete;
    }

    private void ProcessDebugAutoComplete()
    {
        if (!Application.isPlaying || navigationRunState == null) return;

        for (int i = 0; i < zones.Count; i++)
        {
            ZoneDefinition zone = zones[i];
            if (!zone.debugAutoComplete) continue;

            zone.debugAutoComplete = false;
            navigationRunState.DebugCompleteZone(i, zone.requiredCoresMax);
        }
    }
#endif

    public IslandSizeRange GetSizeRange(IslandSizeCategory category) => category switch
    {
        IslandSizeCategory.Small => new IslandSizeRange(radiusSmallMin, radiusSmallMax),
        IslandSizeCategory.Medium => new IslandSizeRange(radiusMediumMin, radiusMediumMax),
        IslandSizeCategory.Large => new IslandSizeRange(radiusLargeMin, radiusLargeMax),
        _ => new IslandSizeRange(radiusSmallMin, radiusSmallMax)
    };
}
