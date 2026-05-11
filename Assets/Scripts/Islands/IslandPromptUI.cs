using UnityEngine;
using TMPro;

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
        islandPromptText.enabled = isNear;
    }
}
