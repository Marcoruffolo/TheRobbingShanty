using UnityEngine;
using UnityEngine.Events;

public class ChestInventory : InventoryHolder, IInteractable
{
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public string InteractionPrompt => string.Empty;

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        OnDynamicInventoryDisplayRequested?.Invoke(primaryInventorySystem);
        interactSuccessful = true;
    }

    public void Interact()
    {
        Interact(null, out _);
    }

    public void EndInteraction()
    {
        OnDynamicInventoryCloseRequested?.Invoke();
        OnInteractionComplete?.Invoke(this);
    }
}