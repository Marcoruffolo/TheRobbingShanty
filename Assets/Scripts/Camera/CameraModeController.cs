using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.Splines;

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

    [Header("Ship Entry Transition")]
    [SerializeField] private CinemachineSplineDolly shipEntryDolly;
    [SerializeField] private float shipEntryDuration = 1.1f;
    [SerializeField] private AnimationCurve shipEntryCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Camera Priorities")]
    [SerializeField] private int activeCameraPriority = 100;
    [SerializeField] private int inactiveCameraPriority = 0;

    private CharacterController _playerController;
    private Transform _defaultPlayerParent;
    private Transform _activeSteeringPosition;
    private Transform _activeExitPosition;
    private Coroutine _shipTransitionRoutine;

    public bool IsTransitionInProgress => _shipTransitionRoutine != null;

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
        if (IsShipControlActive || IsTransitionInProgress)
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

        if (CanPlayShipEntryTransition())
        {
            _shipTransitionRoutine = StartCoroutine(EnterShipControlRoutine());
            return;
        }

        IsShipControlActive = true;
        ApplyShipMode();
    }

    public void ExitShipControl()
    {
        if (!IsShipControlActive || IsTransitionInProgress)
            return;

        Transform releasePoint = _activeExitPosition != null ? _activeExitPosition : _activeSteeringPosition;

        if (CanPlayShipEntryTransition())
        {
            _shipTransitionRoutine = StartCoroutine(ExitShipControlRoutine(releasePoint));
            return;
        }

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

    private IEnumerator EnterShipControlRoutine()
    {
        PrepareShipEntryTransition();

        yield return PlayShipTransition(0f, 1f);

        _shipTransitionRoutine = null;

        IsShipControlActive = true;
        ApplyShipMode();
    }

    private IEnumerator ExitShipControlRoutine(Transform releasePoint)
    {
        PrepareShipExitTransition();

        yield return PlayShipTransition(shipEntryDolly.CameraPosition, 0f);

        _shipTransitionRoutine = null;

        DetachPlayerFromSteeringPoint();

        if (releasePoint != null)
            playerMovement.TeleportTo(releasePoint.position, releasePoint.rotation);

        ClearActiveShipControlData();

        IsShipControlActive = false;
        ApplyPlayerMode();
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

    private bool CanPlayShipEntryTransition()
    {
        return shipCameraRig != null
            && shipEntryDolly != null
            && shipEntryDolly.Spline != null
            && shipEntryDuration > 0f;
    }

    private void PrepareShipEntryTransition()
    {
        shipController.StopControlling();

        _playerController.enabled = false;
        playerMovement.enabled = false;
        playerCamera.enabled = false;

        shipEntryDolly.PositionUnits = PathIndexUnit.Normalized;
        shipEntryDolly.CameraPosition = 0f;

        SetCameraPriorities(shipCameraRig, playerCameraRig);
        PlayerCamera.LockCursor(true);
    }

    private void PrepareShipExitTransition()
    {
        shipController.StopControlling();

        _playerController.enabled = false;
        playerMovement.enabled = false;
        playerCamera.enabled = false;

        shipEntryDolly.PositionUnits = PathIndexUnit.Normalized;
        SetCameraPriorities(shipCameraRig, playerCameraRig);
        PlayerCamera.LockCursor(true);
    }

    private IEnumerator PlayShipTransition(float startPosition, float endPosition)
    {
        float duration = Mathf.Max(0.01f, shipEntryDuration);
        float elapsed = 0f;

        startPosition = Mathf.Clamp01(startPosition);
        endPosition = Mathf.Clamp01(endPosition);
        shipEntryDolly.CameraPosition = startPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float normalizedTime = Mathf.Clamp01(elapsed / duration);
            float curveTime = EvaluateShipEntryPosition(normalizedTime);

            shipEntryDolly.CameraPosition = Mathf.Lerp(startPosition, endPosition, curveTime);

            yield return null;
        }

        shipEntryDolly.CameraPosition = endPosition;
    }

    private float EvaluateShipEntryPosition(float normalizedTime)
    {
        normalizedTime = Mathf.Clamp01(normalizedTime);

        if (shipEntryCurve == null || shipEntryCurve.length == 0)
            return normalizedTime;

        return Mathf.Clamp01(shipEntryCurve.Evaluate(normalizedTime));
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

        if (shipEntryDolly == null && shipCameraRig != null)
            shipEntryDolly = shipCameraRig.GetComponent<CinemachineSplineDolly>();
    }


    private void SetCameraPriorities(CinemachineCamera activeCamera, CinemachineCamera inactiveCamera)
    {
        activeCamera.Priority = activeCameraPriority;
        inactiveCamera.Priority = inactiveCameraPriority;
        activeCamera.Prioritize();
    }
}
