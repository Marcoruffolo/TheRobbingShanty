using UnityEngine;
using System.Collections.Generic;

public class ZoneLayoutResolver : MonoBehaviour
{
    public static ZoneLayoutResolver Instance { get; private set; }

    [SerializeField] private WorldZoneSequence zoneSequence;

    private List<ZoneRange> _ranges = new();
    public IReadOnlyList<ZoneRange> Ranges => _ranges;
    public float WorldDepth => zoneSequence != null ? zoneSequence.TotalDepthZ : 0f;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        Resolve();
    }

    private void Resolve()
    {
        _ranges = zoneSequence != null
            ? new List<ZoneRange>(zoneSequence.ResolveRanges())
            : new List<ZoneRange>();
    }

    public ZoneRange? GetZoneAt(float z)
    {
        foreach (var range in _ranges)
            if (range.Contains(z)) return range;
        return null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (zoneSequence == null) return;

        var ranges = zoneSequence.ResolveRanges();
        for (int i = 0; i < ranges.Count; i++)
        {
            var r = ranges[i];
            Gizmos.color = Color.HSVToRGB((i * 0.15f) % 1f, 0.6f, 0.9f);
            DrawWireRect(new Vector2(r.definition.originX, r.startZ),
                         new Vector2(r.definition.widthX, r.endZ - r.startZ));
            UnityEditor.Handles.Label(
                new Vector3(r.definition.originX + r.definition.widthX * 0.5f, 0f,
                            (r.startZ + r.endZ) * 0.5f),
                $"Zona{i + 1} ({r.definition.biome})");
        }
    }

    private void DrawWireRect(Vector2 origin, Vector2 size)
    {
        Vector3 a = new(origin.x, 0f, origin.y);
        Vector3 b = new(origin.x + size.x, 0f, origin.y);
        Vector3 c = new(origin.x + size.x, 0f, origin.y + size.y);
        Vector3 d = new(origin.x, 0f, origin.y + size.y);
        Gizmos.DrawLine(a, b); Gizmos.DrawLine(b, c);
        Gizmos.DrawLine(c, d); Gizmos.DrawLine(d, a);
    }
#endif
}
