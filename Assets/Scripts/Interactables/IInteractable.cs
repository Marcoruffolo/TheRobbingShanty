using UnityEngine;
using UnityEngine.Events;

public interface IInteractable
{

    string InteractionPrompt { get; }

    UnityAction<IInteractable> OnInteractionComplete { get; set; }

    void Interact(Interactor interactor, out bool interactSuccessful);
    void Interact();
    void EndInteraction();
}