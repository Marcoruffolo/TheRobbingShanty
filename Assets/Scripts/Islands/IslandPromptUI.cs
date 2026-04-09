using UnityEngine;
using TMPro;

/// Muestra u oculta el prompt de isla en el HUD.
/// Poné esto en el CanvasHUD junto con un TextMeshPro para el prompt.

public class IslandPromptUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI islandPromptText;

    private void OnEnable()
    {
        IslandDetector.OnIslandProximityChanged += HandleProximityChange;
    }

    private void OnDisable()
    {
        IslandDetector.OnIslandProximityChanged -= HandleProximityChange;
    }

    private void HandleProximityChange(bool isNear, string promptText)
    {
        islandPromptText.text = promptText;
        islandPromptText.gameObject.SetActive(isNear);
    }
}