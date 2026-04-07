using UnityEngine;
using TMPro;

/// <summary>
/// Detecta objetos interactuables en frente del jugador con un raycast.
/// Muestra el prompt en pantalla y llama a Interact() cuando se presiona E.
/// 
/// Setup: agregá este script a la Main Camera del jugador.
/// Asigná el TxtPrompt desde el Inspector (un Text TMP en el HUD).
/// </summary>
public class InteractionDetector : MonoBehaviour
{
    [Header("Detección")]
    [SerializeField] private float interactionRange = 3f;
    [SerializeField] private LayerMask interactionMask;

    [Header("HUD")]
    [SerializeField] private TMP_Text txtPrompt;

    private IInteractable _currentInteractable;

    // ─────────────────────────────────────────────────────────────
    private void Start()
    {
        if (PlayerInputHandler.Instance != null)
            PlayerInputHandler.Instance.OnInteract += TryInteract;

        if (txtPrompt != null)
            txtPrompt.gameObject.SetActive(false);
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

    // ── Detección ─────────────────────────────────────────────────
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

    // ── Interacción ───────────────────────────────────────────────
    private void TryInteract()
    {
        _currentInteractable?.Interact();
    }

    // ── HUD ───────────────────────────────────────────────────────
    private void ShowPrompt(string text)
    {
        if (txtPrompt == null) return;
        txtPrompt.gameObject.SetActive(true);
        txtPrompt.text = $"[E] {text}";
    }

    private void HidePrompt()
    {
        if (txtPrompt == null) return;
        txtPrompt.gameObject.SetActive(false);
    }

    // ── Gizmo ─────────────────────────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * interactionRange);
    }
}
