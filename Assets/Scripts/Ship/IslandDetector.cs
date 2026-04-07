using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Detecta cuando el barco se acerca a una isla y carga la escena correspondiente.
/// Poné este script en el barco y configurá un trigger grande alrededor de la isla
/// con el tag "Island".
/// 
/// El nombre de la escena a cargar está en el trigger de la isla
/// (componente IslandSceneData).
/// </summary>
public class IslandDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Island")) return;

        IslandSceneData islandData = other.GetComponent<IslandSceneData>();

        if (islandData != null)
        {
            SceneManager.LoadScene(islandData.sceneName);
        }
        else
        {
            Debug.LogWarning("[IslandDetector] El trigger de isla no tiene IslandSceneData.");
        }
    }
}
