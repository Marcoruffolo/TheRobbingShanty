using UnityEngine;


/// Objeto interactuable: el timón del barco.
/// Al interactuar cambia al modo de navegación en tercera persona.
/// Al volver a interactuar (o presionar E de nuevo) devuelve el control al jugador.
/// 
/// Implementa IInteractable — el jugador no sabe que es un timón,
/// solo llama a Interact().

public class SteeringWheel : MonoBehaviour, IInteractable
{
    [Header("Referencias")]
    [SerializeField] private ShipController shipController;
    [SerializeField] private ShipCameraController shipCamera;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Transform steeringPosition;
    [SerializeField] private Camera playerCamera;

    private bool _isControlling = false;

    public string InteractionPrompt => _isControlling ? "Soltar timón" : "Tomar timón";

    public void Interact()
    {
        if (_isControlling)
            ReleaseWheel();
        else
            TakeWheel();
    }

    private void TakeWheel()
    {
        _isControlling = true;
        shipCamera.SetPlayerCamera(playerCamera); // pasar la referencia antes de desactivar
        playerObject.SetActive(false);
        shipController.StartControlling();
        shipCamera.Activate();
    }

    private void ReleaseWheel()
    {
        _isControlling = false;
        playerObject.SetActive(true);

        // Si tiene CharacterController, desactívalo antes de mover
        var cc = playerObject.GetComponent<CharacterController>();
        if (cc != null)
        {
            cc.enabled = false;
            playerObject.transform.position = steeringPosition.position;
            cc.enabled = true;
        }
        else
        {
            playerObject.transform.position = steeringPosition.position;
        }

        shipController.StopControlling();
        shipCamera.Deactivate();
    }
}
