using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private Camera playerCamera;
    public float InteractionRange = 3f;
    public LayerMask InteractableLayerMask;

    public bool IsInteracting { get; private set; }

    private IInteractable currentInteractable;
    private PlayerInventoryHolder playerInventory;

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        playerInventory = GetComponentInParent<PlayerInventoryHolder>();
    }

    public void RequestEndInteraction()
    {
        if (IsInteracting && currentInteractable != null)
            EndInteraction(currentInteractable);
    }

    private void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, InteractionRange, InteractableLayerMask);

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (hitSomething)
            {
                // Try pick up Item first
                var item = hit.collider.GetComponent<Item>();
                if (item != null)
                {
                    item.PickUpItem(playerInventory);
                    return;
                }

                // Then try IInteractable (e.g. chests)
                IInteractable lookingAt = hit.collider.GetComponent<IInteractable>();
                if (!IsInteracting && lookingAt != null)
                {
                    StartInteraction(lookingAt);
                }
                else if (IsInteracting && currentInteractable != null)
                {
                    EndInteraction(currentInteractable);
                }
            }
            else if (IsInteracting && currentInteractable != null)
            {
                // Pressed E while not looking at anything — close active interaction
                EndInteraction(currentInteractable);
            }
        }
    }

    private void StartInteraction(IInteractable interactable)
    {
        interactable.Interact(this, out bool interactSuccessful);
        if (interactSuccessful)
        {
            currentInteractable = interactable;
            IsInteracting = true;
        }
    }

    private void EndInteraction(IInteractable interactable)
    {
        interactable.EndInteraction();
        currentInteractable = null;
        IsInteracting = false;
    }

    private void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * InteractionRange);
        }
    }
}