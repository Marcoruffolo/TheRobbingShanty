using UnityEngine;


/// Datos de una isla. Va en el GameObject del trigger de la isla.
/// Define qué escena cargar cuando el barco se acerca.

public class IslandSceneData : MonoBehaviour
{
    [Tooltip("Nombre exacto de la escena a cargar (debe estar en Build Settings)")]
    public string sceneName;
}
