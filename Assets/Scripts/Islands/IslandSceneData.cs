using UnityEngine;

/// <summary>
/// Datos de una isla. Va en el GameObject del trigger de la isla.
/// Define qué escena cargar cuando el barco se acerca.
/// </summary>
public class IslandSceneData : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena a cargar (debe estar en Build Settings)")]
    public string sceneName;

    [Tooltip("Nombre visible en el prompt (opcional, ej: 'Castle')")]
    public string islandName;
}
