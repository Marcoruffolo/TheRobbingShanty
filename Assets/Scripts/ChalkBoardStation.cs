using UnityEngine;
using UnityEngine.Events;

public class ChalkBoardStation : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Mejorar Items";
    public string InteractionPrompt => interactionPrompt;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public BoolGameEvent OnUpgradeUIOpen;
    public BoolGameEvent OnItemUpgradeUIOpen;

    private void Start() 
    {
        OnUpgradeUIOpen?.Raise(false);
    }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        PlayerCamera.LockCursor(false);
        BlockPlayerMovement.Instance?.ImmobilizePlayer();
        OnUpgradeUIOpen?.Raise(true);
        interactSuccessful = true;
    }

    public void EndInteraction()
    {
        OnItemUpgradeUIOpen?.Raise(false);
        PlayerCamera.LockCursor(true);
        BlockPlayerMovement.Instance?.FreePlayer(); 
        OnUpgradeUIOpen?.Raise(false);
    }

    public void Interact() => Interact(null, out _);
}
