using UnityEngine;

/// <summary>
/// Objeto interactuable: el timón del barco.
/// Al interactuar cambia al modo de navegación en tercera persona.
/// Al volver a interactuar (o presionar E de nuevo) devuelve el control al jugador.
/// 
/// Implementa IInteractable — el jugador no sabe que es un timón,
/// solo llama a Interact().
/// </summary>
public class SteeringWheel : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    [SerializeField] private ShipController   shipController;
    [SerializeField] private ShipCameraController shipCamera;
    [SerializeField] private GameObject       playerObject;   // el GO del jugador

    [Header("Posición del jugador al tomar el timón")]
    [SerializeField] private Transform        steeringPosition; // Transform vacío frente al timón

    private bool _isControlling = false;

    // ── IInteractable ─────────────────────────────────────────────
    public string InteractionPrompt => _isControlling ? "Soltar timón" : "Tomar timón";

    public void Interact()
    {
        if (_isControlling)
            ReleaseWheel();
        else
            TakeWheel();
    }

    // ─────────────────────────────────────────────────────────────
    private void TakeWheel()
    {
        _isControlling = true;

        // Mover y bloquear al jugador en la posición del timón
        if (steeringPosition != null && playerObject != null)
        {
            playerObject.transform.position = steeringPosition.position;
            playerObject.transform.rotation = steeringPosition.rotation;
        }

        // Desactivar movimiento del jugador y activar control del barco
        PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
        if (playerMovement != null) playerMovement.enabled = false;

        PlayerCamera playerCamera = playerObject.GetComponentInChildren<PlayerCamera>();
        if (playerCamera != null) playerCamera.enabled = false;

        shipController.StartControlling();
        shipCamera.Activate();

        PlayerCamera.LockCursor(false);
    }

    private void ReleaseWheel()
    {
        _isControlling = false;

        // Reactivar movimiento del jugador
        PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
        if (playerMovement != null) playerMovement.enabled = true;

        PlayerCamera playerCamera = playerObject.GetComponentInChildren<PlayerCamera>();
        if (playerCamera != null) playerCamera.enabled = true;

        shipController.StopControlling();
        shipCamera.Deactivate();

        PlayerCamera.LockCursor(true);
    }
}
