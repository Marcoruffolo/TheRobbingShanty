using UnityEngine;

public class IslandWorldGizmos : MonoBehaviour
{
    [SerializeField] private IslandWorldConfig config;
    [SerializeField] private bool showLabels = true;

    private void OnDrawGizmos()
    {
        if (config == null) return;

        DrawForbiddenZones();
        DrawAllowedZones();
        DrawManualIslands();
    }

    private void DrawForbiddenZones()
    {
        foreach (var fr in config.forbiddenRects)
        {
            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.25f);
            DrawSolidRect(fr.rect);
            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.9f);
            DrawWireRect(new Vector2(fr.rect.x, fr.rect.y),
                         new Vector2(fr.rect.width, fr.rect.height));
#if UNITY_EDITOR
            if (showLabels)
                UnityEditor.Handles.Label(
                    new Vector3(fr.rect.center.x, 0f, fr.rect.center.y), $"✗ {fr.label}");
#endif
        }

        foreach (var fc in config.forbiddenCircles)
        {
            Vector3 c = new(fc.center.x, 0f, fc.center.y);
            Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.9f);
            Gizmos.DrawWireSphere(c, fc.radius);
#if UNITY_EDITOR
            if (showLabels) UnityEditor.Handles.Label(c, $"✗ {fc.label}");
#endif
        }
    }

    private void DrawAllowedZones()
    {
        foreach (var ar in config.allowedRects)
        {
            Gizmos.color = new Color(0.15f, 1f, 0.15f, 0.15f);
            DrawSolidRect(ar.rect);
            Gizmos.color = new Color(0.15f, 1f, 0.15f, 0.9f);
            DrawWireRect(new Vector2(ar.rect.x, ar.rect.y),
                         new Vector2(ar.rect.width, ar.rect.height));
#if UNITY_EDITOR
            if (showLabels)
                UnityEditor.Handles.Label(
                    new Vector3(ar.rect.center.x, 0f, ar.rect.center.y), $"✓ {ar.label}");
#endif
        }

        foreach (var ac in config.allowedCircles)
        {
            Vector3 c = new(ac.center.x, 0f, ac.center.y);
            Gizmos.color = new Color(0.15f, 1f, 0.15f, 0.9f);
            Gizmos.DrawWireSphere(c, ac.radius);
#if UNITY_EDITOR
            if (showLabels) UnityEditor.Handles.Label(c, $"✓ {ac.label}");
#endif
        }
    }

    private void DrawManualIslands()
    {
        foreach (var mi in config.manualIslands)
        {
            Vector3 pos = new(mi.positionXZ.x, 0f, mi.positionXZ.y);
            float radius = GetApproxRadius(mi.size);
            Gizmos.color = new Color(1f, 0.85f, 0f, 0.9f);
            Gizmos.DrawWireSphere(pos, radius);
            Gizmos.DrawSphere(pos, 8f);
#if UNITY_EDITOR
            if (showLabels)
                UnityEditor.Handles.Label(pos + Vector3.up * 5f,
                    $"★ {mi.displayName}\n({mi.size})");
#endif
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

    private void DrawSolidRect(Rect r)
    {
        int lines = 20;
        for (int i = 0; i <= lines; i++)
        {
            float t = (float)i / lines;
            float z = Mathf.Lerp(r.yMin, r.yMax, t);
            Gizmos.DrawLine(new Vector3(r.xMin, 0f, z), new Vector3(r.xMax, 0f, z));
        }
    }

    private float GetApproxRadius(IslandSizeCategory cat) => cat switch
    {
        IslandSizeCategory.Small => 60f,
        IslandSizeCategory.Medium => 110f,
        IslandSizeCategory.Large => 170f,
        _ => 60f
    };
}