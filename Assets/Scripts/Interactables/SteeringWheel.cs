using UnityEngine;

public class SteeringWheel : MonoBehaviour, IInteractable
{
    [Header("Positions")]
    [SerializeField] private Transform steeringPosition;
    [SerializeField] private Transform exitPosition;

    private bool _releaseInputSubscribed;

    public string InteractionPrompt
    {
        get
        {
            if (CameraModeController.Instance != null && CameraModeController.Instance.IsShipControlActive)
                return "Leave helm";

            return "Take helm";
        }
    }

    public void Interact()
    {
        CameraModeController controller = CameraModeController.Instance;

        if (controller == null)
        {
            Debug.LogError("[SteeringWheel] No CameraModeController instance found.");
            return;
        }

        if (controller.IsShipControlActive)
        {
            ExitShipControl();
            return;
        }

        EnterShipControl();
    }

    private void OnDisable()
    {
        UnsubscribeFromReleaseInput();
    }

    private void OnDestroy()
    {
        UnsubscribeFromReleaseInput();
    }

    private void EnterShipControl()
    {
        if (steeringPosition == null)
        {
            Debug.LogError("[SteeringWheel] Missing steeringPosition reference.");
            return;
        }

        Transform safeExitPosition = exitPosition != null ? exitPosition : steeringPosition;

        CameraModeController.Instance.EnterShipControl(steeringPosition, safeExitPosition);
        SubscribeToReleaseInput();
    }

    private void ExitShipControl()
    {
        CameraModeController.Instance.ExitShipControl();
        UnsubscribeFromReleaseInput();
    }

    private void HandleReleaseInput()
    {
        CameraModeController controller = CameraModeController.Instance;

        if (controller == null || !controller.IsShipControlActive)
            return;

        ExitShipControl();
    }

    private void SubscribeToReleaseInput()
    {
        if (_releaseInputSubscribed)
            return;

        if (PlayerInputHandler.Instance == null)
        {
            Debug.LogError("[SteeringWheel] No PlayerInputHandler instance found.");
            return;
        }

        PlayerInputHandler.Instance.OnInteract += HandleReleaseInput;
        _releaseInputSubscribed = true;
    }

    private void UnsubscribeFromReleaseInput()
    {
        if (!_releaseInputSubscribed)
            return;

        if (PlayerInputHandler.Instance != null)
            PlayerInputHandler.Instance.OnInteract -= HandleReleaseInput;

        _releaseInputSubscribed = false;
    }
}