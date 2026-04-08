using UnityEngine;
using Unity.Cinemachine;

public class CameraModeController : MonoBehaviour
{
    public static CameraModeController Instance { get; private set; }

    public bool IsShipControlActive { get; private set; }

    [Header("Player References")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerCamera playerCamera;

    [Header("Ship References")]
    [SerializeField] private ShipController shipController;

    [Header("Cinemachine References")]
    [SerializeField] private CinemachineCamera playerCameraRig;
    [SerializeField] private CinemachineCamera shipCameraRig;

    [Header("Camera Priorities")]
    [SerializeField] private int activeCameraPriority = 100;
    [SerializeField] private int inactiveCameraPriority = 0;

    private CharacterController _playerController;
    private Transform _defaultPlayerParent;
    private Transform _activeSteeringPosition;
    private Transform _activeExitPosition;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        ResolveReferences();

        if (playerObject != null)
            _defaultPlayerParent = playerObject.transform.parent;
    }

    private void Start()
    {
        RestoreGameplayState();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void EnterShipControl(Transform steeringPosition)
    {
        EnterShipControl(steeringPosition, steeringPosition);
    }

    public void EnterShipControl(Transform steeringPosition, Transform exitPosition)
    {
        if (IsShipControlActive)
            return;

        if (steeringPosition == null)
        {
            Debug.LogError("[CameraModeController] Missing steeringPosition reference.");
            return;
        }

        _activeSteeringPosition = steeringPosition;
        _activeExitPosition = exitPosition != null ? exitPosition : steeringPosition;

        playerMovement.TeleportTo(steeringPosition.position, steeringPosition.rotation);
        AttachPlayerToSteeringPoint();

        IsShipControlActive = true;
        ApplyShipMode();
    }

    public void ExitShipControl()
    {
        if (!IsShipControlActive)
            return;

        Transform releasePoint = _activeExitPosition != null ? _activeExitPosition : _activeSteeringPosition;

        DetachPlayerFromSteeringPoint();

        if (releasePoint != null)
            playerMovement.TeleportTo(releasePoint.position, releasePoint.rotation);

        ClearActiveShipControlData();

        IsShipControlActive = false;
        ApplyPlayerMode();
    }

    public void ToggleShipControl(Transform steeringPosition)
    {
        if (IsShipControlActive)
            ExitShipControl();
        else
            EnterShipControl(steeringPosition);
    }

    public void ToggleShipControl(Transform steeringPosition, Transform exitPosition)
    {
        if (IsShipControlActive)
            ExitShipControl();
        else
            EnterShipControl(steeringPosition, exitPosition);
    }

    public void RestoreGameplayState()
    {

        if (IsShipControlActive)
        {
            AttachPlayerToSteeringPoint();
            ApplyShipMode();
        }
        else
        {
            DetachPlayerFromSteeringPoint();
            ApplyPlayerMode();
        }
    }

    private void ApplyPlayerMode()
    {
        shipController.StopControlling();

        _playerController.enabled = true;
        playerMovement.enabled = true;
        playerCamera.enabled = true;

        SetCameraPriorities(playerCameraRig, shipCameraRig);
        PlayerCamera.LockCursor(true);
    }

    private void ApplyShipMode()
    {
        _playerController.enabled = false;
        playerMovement.enabled = false;
        playerCamera.enabled = false;

        shipController.StartControlling();

        SetCameraPriorities(shipCameraRig, playerCameraRig);
        PlayerCamera.LockCursor(true);
    }

    private void AttachPlayerToSteeringPoint()
    {
        if (playerObject == null || _activeSteeringPosition == null)
            return;

        Transform playerTransform = playerObject.transform;

        playerTransform.SetParent(_activeSteeringPosition, true);
        playerTransform.localPosition = Vector3.zero;
        playerTransform.localRotation = Quaternion.identity;
    }

    private void DetachPlayerFromSteeringPoint()
    {
        if (playerObject == null)
            return;

        playerObject.transform.SetParent(_defaultPlayerParent, true);
    }

    private void ClearActiveShipControlData()
    {
        _activeSteeringPosition = null;
        _activeExitPosition = null;
    }

    private void ResolveReferences()
    {
        if (playerObject == null && playerMovement != null)
            playerObject = playerMovement.gameObject;

        if (playerMovement == null && playerObject != null)
            playerMovement = playerObject.GetComponent<PlayerMovement>();

        if (playerCamera == null && playerObject != null)
            playerCamera = playerObject.GetComponentInChildren<PlayerCamera>(true);

        if (_playerController == null && playerObject != null)
            _playerController = playerObject.GetComponent<CharacterController>();

        if (shipController == null)
            shipController = FindFirstObjectByType<ShipController>();
    }


    private void SetCameraPriorities(CinemachineCamera activeCamera, CinemachineCamera inactiveCamera)
    {
        activeCamera.Priority = activeCameraPriority;
        inactiveCamera.Priority = inactiveCameraPriority;
        activeCamera.Prioritize();
    }
}