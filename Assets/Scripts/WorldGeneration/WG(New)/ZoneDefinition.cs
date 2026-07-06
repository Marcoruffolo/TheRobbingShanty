using UnityEngine;

[System.Serializable]
public class ZoneDefinition
{
    private const int DefaultRequiredCoresMin = 5;
    private const int DefaultRequiredCoresMax = 7;
    private const int DefaultExtraCoreIslandsMin = 2;
    private const int DefaultExtraCoreIslandsMax = 3;

    [Tooltip("Bioma de esta zona: determina que islas pueden spawnear en ella")]
    public ZoneBiome biome;

    [Tooltip("Largo de la zona en el eje de avance (Z) del overworld")]
    public float lengthZ = 200f;

    [Header("Banda en X")]
    public float originX = 0f;
    public float widthX = 500f;

    public float gateClearanceZ = 40f;

    [Header("Nucleos requeridos")]
    public int requiredCoresMin = DefaultRequiredCoresMin;
    public int requiredCoresMax = DefaultRequiredCoresMax;
    public int extraCoreIslandsMin = DefaultExtraCoreIslandsMin;
    public int extraCoreIslandsMax = DefaultExtraCoreIslandsMax;

    [Tooltip("Distancia minima entre islas de artefacto dentro de esta zona")]
    public float minArtifactDistance = 30f;

    [Header("Proporciones de tamano (islas random)")]
    public int weightSmall = 1;
    public int weightMedium = 1;
    public int weightLarge = 1;

    public int RollRequiredCores(System.Random rng)
    {
        int min = Mathf.Max(1, requiredCoresMin);
        int max = Mathf.Max(min, requiredCoresMax);
        return rng.Next(min, max + 1);
    }

    public int RollExtraCoreIslands(System.Random rng)
    {
        int min = Mathf.Max(0, extraCoreIslandsMin);
        int max = Mathf.Max(min, extraCoreIslandsMax);
        return rng.Next(min, max + 1);
    }

    public IslandSizeCategory GetWeightedRandomSize(System.Random rng)
    {
        int small = Mathf.Max(0, weightSmall);
        int medium = Mathf.Max(0, weightMedium);
        int large = Mathf.Max(0, weightLarge);
        int total = small + medium + large;
        if (total <= 0) return IslandSizeCategory.Small;

        int r = rng.Next(total);

        if (r < small) return IslandSizeCategory.Small;
        r -= small;
        if (r < medium) return IslandSizeCategory.Medium;
        return IslandSizeCategory.Large;
    }
}