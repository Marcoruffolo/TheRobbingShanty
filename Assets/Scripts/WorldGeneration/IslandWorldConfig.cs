using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "IslandWorldConfig", menuName = "TRS/Island World Config")]
public class IslandWorldConfig : ScriptableObject
{
    [Header("Océano")]
    public Vector2 worldSize = new Vector2(5000f, 5000f);
    public Vector2 worldOrigin = Vector2.zero;

    [Header("Seed")]
    [Tooltip("0 = aleatorio cada partida. Valor fijo = mundo siempre igual.")]
    public int globalSeed = 0;

    [Header("Islas Procedurales")]
    [Range(1, 300)] public int targetIslandCount = 40;
    [Tooltip("Cuántos intentos hace antes de rendirse si no encuentra posición válida")]
    public int maxPlacementAttempts = 1000;
    [Tooltip("Distancia mínima entre el BORDE de dos islas")]
    public float minDistanceBetweenIslands = 150f;

    [Header("Tamaños — Radio visual en el overworld")]
    public IslandSizeRange tinyRange = new IslandSizeRange(20, 40);
    public IslandSizeRange smallRange = new IslandSizeRange(40, 80);
    public IslandSizeRange mediumRange = new IslandSizeRange(80, 140);
    public IslandSizeRange largeRange = new IslandSizeRange(140, 200);
    public IslandSizeRange hugeRange = new IslandSizeRange(200, 300);

    [Header("Distribución de tamaños (pesos relativos)")]
    [Range(0, 100)] public int weightTiny = 20;
    [Range(0, 100)] public int weightSmall = 35;
    [Range(0, 100)] public int weightMedium = 30;
    [Range(0, 100)] public int weightLarge = 12;
    [Range(0, 100)] public int weightHuge = 3;

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