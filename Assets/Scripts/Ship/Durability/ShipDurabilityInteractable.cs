using UnityEngine;
using UnityEngine.Events;

public class ShipDurabilityInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Repair Hull";

    public string InteractionPrompt => interactionPrompt;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public static UnityAction OnDurabilityUIOpened;
    public static UnityAction OnDurabilityUIClosed;

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        PlayerCamera.LockCursor(false);
        BlockPlayerMovement.Instance?.ImmobilizePlayer();
        OnDurabilityUIOpened?.Invoke();
        interactSuccessful = true;
    }

    public void Interact() => Interact(null, out _);

    public void EndInteraction()
    {
        OnDurabilityUIClosed?.Invoke();
        PlayerCamera.LockCursor(true);
        BlockPlayerMovement.Instance?.FreePlayer();
        OnInteractionComplete?.Invoke(this);
    }
}
