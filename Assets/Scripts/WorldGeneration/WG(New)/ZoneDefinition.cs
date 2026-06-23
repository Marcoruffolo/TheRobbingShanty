using UnityEngine;

[System.Serializable]
public class ZoneDefinition
{
    [Tooltip("Bioma de esta zona: determina qué islas premade pueden spawnear en ella")]
    public ZoneBiome biome;

    [Tooltip("Largo de la zona en el eje de avance (Z) del overworld")]
    public float lengthZ = 200f;

    [Header("Banda en X")]
    public float originX = 0f;
    public float widthX = 500f;

    [Header("Islas-artefacto (cantidad random por run)")]
    public int artifactCountMin = 1;
    public int artifactCountMax = 3;
    [Tooltip("Distancia mínima entre islas-artefacto dentro de esta zona")]
    public float minArtifactDistance = 30f;

    [Header("Proporciones de tamaño (islas random)")]
    public int weightSmall = 1;
    public int weightMedium = 1;
    public int weightLarge = 1;

    public IslandSizeCategory GetWeightedRandomSize(System.Random rng)
    {
        int total = weightSmall + weightMedium + weightLarge;
        int r = rng.Next(total);

        if (r < weightSmall) return IslandSizeCategory.Small;
        r -= weightSmall;
        if (r < weightMedium) return IslandSizeCategory.Medium;
        return IslandSizeCategory.Large;
    }
}
