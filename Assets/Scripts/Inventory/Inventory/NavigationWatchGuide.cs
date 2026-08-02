using UnityEngine;

public class NavigationWatchGuide : MonoBehaviour
{
    [SerializeField] private PlayerInventoryHolder playerInventory;
    [SerializeField] private NavigationRunState navigationRunState;
    [SerializeField] private InventoryItemData navigationWatchItem;
    [SerializeField] private InventoryItemData navigationCoreItem;
    [SerializeField] private BoolGameEvent showWatchEvent;
    [SerializeField] private SOVariableString watchText;

    [Header("Textos")]
    [SerializeField] private string claimedArtifactText = "El artefacto de esta isla ya entrego su nucleo.";
    [SerializeField] private string completedArtifactText = "Nucleo liberado. Recogelo antes de volver.";
    [SerializeField] private string detectedArtifactFormat = "Nucleo detectado. Te faltan {0} enemigos para liberarlo.";
    [SerializeField] private string carriedCoresFormat = "Tenes {0} nucleo(s) sin vincular. Vinculalos en la mesa. Mesa: {1}/{2}.";
    [SerializeField] private string completedTableText = "Mesa completa. La muralla ya puede abrirse.";
    [SerializeField] private string progressFormat = "Nucleos vinculados: {0}/{1}. Faltan {2}. Artefactos disponibles: {3}.";

    private bool _isShowing;
    private NavigationRunState State => navigationRunState != null ? navigationRunState : NavigationRunState.Instance;

    private void Awake()
    {
        if (playerInventory == null)
            playerInventory = GetComponent<PlayerInventoryHolder>();
    }

    private void LateUpdate()
    {
        if (playerInventory == null || navigationWatchItem == null)
        {
            HideWatch();
            return;
        }

        if (playerInventory.SelectedItemData != navigationWatchItem)
        {
            HideWatch();
            return;
        }

        string text = BuildWatchText();
        if (string.IsNullOrWhiteSpace(text))
            HideWatch();
        else
            ShowWatch(text);
    }

    private string BuildWatchText()
    {
        NavigationRunState state = State;
        if (state == null) return string.Empty;

        string artifactId = state.CurrentArtifactIslandId;
        if (!string.IsNullOrWhiteSpace(artifactId))
        {
            if (state.IsCoreClaimed(artifactId))
                return claimedArtifactText;

            if (state.IsArtifactCompleted(artifactId))
                return completedArtifactText;

            return string.Format(detectedArtifactFormat, state.GetArtifactKillsRemaining(artifactId));
        }

        int zoneIndex = state.CurrentZoneIndex;
        if (!state.IsZoneReady(zoneIndex))
            return string.Empty;

        int carriedCores = navigationCoreItem != null ? playerInventory.GetItemCount(navigationCoreItem) : 0;
        int placed = state.GetPlacedCores(zoneIndex);
        int required = state.GetRequiredCores(zoneIndex);
        int available = state.GetAvailableCoreIslandCount(zoneIndex);

        if (carriedCores > 0)
            return string.Format(carriedCoresFormat, carriedCores, placed, required);

        if (placed >= required)
            return completedTableText;

        int remaining = Mathf.Max(0, required - placed);
        return string.Format(progressFormat, placed, required, remaining, available);
    }

    private void ShowWatch(string text)
    {
        if (watchText != null)
            watchText.Value = text;

        showWatchEvent?.Raise(true);
        _isShowing = true;
    }

    private void HideWatch()
    {
        if (!_isShowing) return;

        if (watchText != null)
            watchText.Value = string.Empty;

        showWatchEvent?.Raise(false);
        _isShowing = false;
    }

    private void OnDisable()
    {
        HideWatch();
    }
}