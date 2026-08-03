using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ChalkBoardStation : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Mejorar Items";
    public string InteractionPrompt => MenuEnabled ? interactionPrompt : string.Empty;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public BoolGameEvent OnUpgradeUIOpen;
    public BoolGameEvent OnItemUpgradeUIOpen;
    public BoolGameEvent OnShipUpgradeUIOpen;

    [Header("Cosas a esconder cuando se activa el menu")]
    [SerializeField] private GameObject hotbar;
    [SerializeField] private GameObject healthBar;
    [SerializeField] private GameObject islandPromptText;

    private static bool MenuEnabled => SceneManager.GetActiveScene().name == "MainScene";

    private void Start()
    {
        OnUpgradeUIOpen?.Raise(false);
    }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        interactSuccessful = MenuEnabled;
        if (!interactSuccessful) return;

        PlayerCamera.LockCursor(false);
        BlockPlayerMovement.Instance?.ImmobilizePlayer();
        OnUpgradeUIOpen?.Raise(true);
        SetHudVisible(false);
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
