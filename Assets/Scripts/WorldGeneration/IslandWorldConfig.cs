using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "IslandWorldConfig", menuName = "TRS/Island World Config")]
public class IslandWorldConfig : ScriptableObject
{
    [Header("Zonas PROHIBIDAS — nunca se generan islas aquí")]
    public List<ForbiddenZoneRect> forbiddenRects = new();
    public List<ForbiddenZoneCircle> forbiddenCircles = new();

    [Header("Zonas PERMITIDAS — procedurales solo dentro de estas")]
    [Tooltip("Si ambas listas están vacías, todo el océano es zona válida")]
    public List<AllowedZoneRect> allowedRects = new();
    public List<AllowedZoneCircle> allowedCircles = new();

    [Header("Islas Manuales")]
    public List<ManualIslandEntry> manualIslands = new();
}

[System.Serializable]
public class IslandSizeRange
{
    public float radiusMin;
    public float radiusMax;

    public IslandSizeRange(float min, float max)
    { radiusMin = min; radiusMax = max; }
}

public enum IslandSizeCategory { Tiny, Small, Medium, Large, Huge }

[System.Serializable]
public class ForbiddenZoneRect
{
    public string label = "Zona Prohibida";
    public Rect rect;          // coordenadas XZ del mundo
}

[System.Serializable]
public class ForbiddenZoneCircle
{
    public string label = "Zona Prohibida";
    public Vector2 center;
    public float radius;
}

[System.Serializable]
public class AllowedZoneRect
{
    public string label = "Zona Permitida";
    public Rect rect;
}

[System.Serializable]
public class AllowedZoneCircle
{
    public string label = "Zona Permitida";
    public Vector2 center;
    public float radius;
}

[System.Serializable]
public class ManualIslandEntry
{
    public string instanceId = "island_manual_01";
    public string displayName = "Isla Manual";
    public Vector2 positionXZ;          // posición exacta en el overworld
    public IslandSizeCategory size = IslandSizeCategory.Medium;
    [Tooltip("0 = deriva del globalSeed + instanceId")]
    public int seedOverride = 0;
}