using UnityEngine;
using UnityEngine.InputSystem;

public class Interactor : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] public float InteractionRange = 3f;
    [SerializeField] public LayerMask InteractableLayerMask;

    [Header("HUD (opcional)")]
    [SerializeField] private BoolGameEvent showPromptEvent;
    [SerializeField] private SOVariableString promptText;

    public bool IsInteracting { get; private set; }

    private IInteractable _currentInteractable;
    private RaycastHit _hit;
    private bool _hitSomething;
    private PlayerInventoryHolder _playerInventory;

    private void Awake()
    {
        if (playerCamera == null) playerCamera = Camera.main;
        _playerInventory = GetComponentInParent<PlayerInventoryHolder>();
    }

    public void RequestEndInteraction()
    {
        if (IsInteracting && _currentInteractable != null)
            EndInteraction(_currentInteractable);
    }

    private void Update()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        _hitSomething = Physics.Raycast(ray, out _hit, InteractionRange, InteractableLayerMask);

        UpdatePrompt();

        if (Keyboard.current.eKey.wasPressedThisFrame)
            HandleInput();
    }

    private void UpdatePrompt()
    {
        if (IsInteracting) return;

        if (_hitSomething)
        {
            var label = _hit.collider.GetComponent<InteractionLabel>();
            if (label != null)
            {
                SetPrompt(label.Prompt, true);
                return;
            }

            var interactable = _hit.collider.GetComponent<IInteractable>();
            if (interactable != null && !string.IsNullOrEmpty(interactable.InteractionPrompt))
            {
                SetPrompt(interactable.InteractionPrompt, true);
                return;
            }

            var item = _hit.collider.GetComponent<Item>();
            if (item != null)
            {
                SetPrompt($"[E] Recoger {item.ItemData.DisplayName}", true);
                return;
            }
        }

        SetPrompt("", false);
    }

    private void HandleInput()
    {
        if (_hitSomething)
        {
            var item = _hit.collider.GetComponent<Item>();
            if (item != null)
            {
                item.PickUpItem(_playerInventory);
                return;
            }

            var lookingAt = _hit.collider.GetComponent<IInteractable>();
            if (!IsInteracting && lookingAt != null)
                StartInteraction(lookingAt);
            else if (IsInteracting && _currentInteractable != null)
                EndInteraction(_currentInteractable);
        }
        else if (IsInteracting && _currentInteractable != null)
        {
            EndInteraction(_currentInteractable);
        }
    }

    private void StartInteraction(IInteractable interactable)
    {
        interactable.Interact(this, out bool success);
        if (success)
        {
            _currentInteractable = interactable;
            IsInteracting = true;
        }
    }

    private void EndInteraction(IInteractable interactable)
    {
        interactable.EndInteraction();
        _currentInteractable = null;
        IsInteracting = false;
    }

    private void SetPrompt(string text, bool visible)
    {
        if (promptText != null) promptText.Value = text;
        showPromptEvent?.Raise(visible);
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
