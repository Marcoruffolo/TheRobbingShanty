using UnityEngine;
using UnityEngine.Events;

public class ChalkBoardStation : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Mejorar Items";
    public string InteractionPrompt => interactionPrompt;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public BoolGameEvent OnUpgradeUIOpen;
    public BoolGameEvent OnItemUpgradeUIOpen;
    public BoolGameEvent OnShipUpgradeUIOpen;

    [Header("Cosas a esconder cuando se activa el menu")]
    [SerializeField] private GameObject hotbar;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject islandPromptText;

    private void Start()
    {
        OnUpgradeUIOpen?.Raise(false);
    }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        PlayerCamera.LockCursor(false);
        BlockPlayerMovement.Instance?.ImmobilizePlayer();
        OnUpgradeUIOpen?.Raise(true);
        SetHudVisible(false);
        interactSuccessful = true;
    }

    public void EndInteraction()
    {
        OnItemUpgradeUIOpen?.Raise(false);
        OnShipUpgradeUIOpen?.Raise(false);
        PlayerCamera.LockCursor(true);
        BlockPlayerMovement.Instance?.FreePlayer();
        OnUpgradeUIOpen?.Raise(false);
        SetHudVisible(true);
    }

    private void SetHudVisible(bool visible)
    {
        hotbar?.SetActive(visible);
        healthBar?.SetActive(visible);
        islandPromptText?.SetActive(visible);
    }

    public void Interact() => Interact(null, out _);
}
