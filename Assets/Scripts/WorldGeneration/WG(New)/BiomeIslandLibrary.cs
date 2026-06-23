using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BiomeIslandLibrary", menuName = "TRS/Biome Island Library")]
public class BiomeIslandLibrary : ScriptableObject
{
    [System.Serializable]
    public class BiomeIslandSet
    {
        public ZoneBiome biome;

        [Tooltip("Prefabs de islas-artefacto (con máquina) que pueden spawnear en este bioma")]
        public List<GameObject> artifactIslandPrefabs = new();

        [Tooltip("Prefabs de islas random (relleno) que pueden spawnear en este bioma")]
        public List<GameObject> randomIslandPrefabs = new();
    }

    public List<BiomeIslandSet> biomeSets = new();

    private BiomeIslandSet GetSetFor(ZoneBiome biome)
    {
        foreach (var set in biomeSets)
            if (set.biome == biome) return set;
        return null;
    }

    public List<GameObject> GetArtifactPrefabsFor(ZoneBiome biome) => GetSetFor(biome)?.artifactIslandPrefabs;

    public List<GameObject> GetRandomPrefabsFor(ZoneBiome biome) => GetSetFor(biome)?.randomIslandPrefabs;
}
