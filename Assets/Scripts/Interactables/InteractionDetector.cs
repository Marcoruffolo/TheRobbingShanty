using UnityEngine;
using TMPro;


/// Detecta objetos interactuables en frente del jugador con un raycast.
/// Muestra el prompt en pantalla y llama a Interact() cuando se presiona E.
/// 
/// Setup: agregá este script a la Main Camera del jugador.
/// Asigná el TxtPrompt desde el Inspector (un Text TMP en el HUD).
public class InteractionDetector : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionMask;

    [Header("HUD")]
    [SerializeField] BoolGameEvent ActivateText;
    [SerializeField] SOVariableString interactText;

    private IInteractable _currentInteractable;

    // ─────────────────────────────────────────────────────────────
    private void Start()
    {
        if (PlayerInputHandler.Instance != null)
            PlayerInputHandler.Instance.OnInteract += TryInteract;

        ActivateText.Raise(false);
    }

    private void OnDestroy()
    {
        if (PlayerInputHandler.Instance != null)
            PlayerInputHandler.Instance.OnInteract -= TryInteract;
    }

    private void Update()
    {
        DetectInteractable();
    }

    // Detección
    private void DetectInteractable()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, interactionMask))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                _currentInteractable = interactable;
                ShowPrompt(interactable.InteractionPrompt);
                return;
            }
        }

        _currentInteractable = null;
        HidePrompt();
    }

    // Interacción 
    private void TryInteract()
    {
        _currentInteractable?.Interact();
    }

    // HUD 
    private void ShowPrompt(string text)
    {
        ActivateText.Raise(true);
        interactText.Value = text;
    }

    private void HidePrompt()
    {
        ActivateText.Raise(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * interactionRange);
    }
}
