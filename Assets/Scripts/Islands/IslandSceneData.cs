using UnityEngine;

/// <summary>
/// Datos de una isla. Va en el GameObject del trigger de la isla.
/// Define que escena cargar y, si corresponde, que artefacto representa en la run actual.
/// </summary>
public class IslandSceneData : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena a cargar (debe estar en Build Settings)")]
    public SceneField sceneName;

    [Tooltip("Nombre visible en el prompt")]
    public string islandName;

    [Header("Artefacto")]
    public bool isArtifactIsland;
    public int zoneIndex;
    public string artifactId;
}
