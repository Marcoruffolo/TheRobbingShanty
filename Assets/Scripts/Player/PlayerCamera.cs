using Unity.Cinemachine;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody;

    [Header("Vertical Rotation Limits")]
    [SerializeField] private float minVerticalAngle = -80f;
    [SerializeField] private float maxVerticalAngle = 80f;

    [Header("Look Settings")]
    [SerializeField] private float smoothTime = 0.05f;
    [SerializeField] private float lookInputMultiplier = 0.05f;

    [Header("Zoom / Aim")]
    [SerializeField] private CinemachineCamera vcam;
    [SerializeField] private float aimFOV = 40f;
    [SerializeField] private float zoomSpeed = 10f;

    private float _baseFOV;
    private bool _isAiming;

    private PlayerInputHandler _input;
    private float _xRotation;
    private float _sensitivity;

    private Vector2 _currentLook;
    private Vector2 _lookVelocity;

    public bool IsLocked { get; set; }

    private void Awake()
    {
        if (playerBody == null && transform.parent != null)
            playerBody = transform.parent;

        if (vcam == null) vcam = GetComponentInChildren<CinemachineCamera>();
        if (vcam != null) _baseFOV = vcam.Lens.FieldOfView;
    }

    public void SetAiming(bool aiming) => _isAiming = aiming;

    private void Start()
    {
        _input = PlayerInputHandler.Instance;

        if (_input == null)
        {
            Debug.LogError("[PlayerCamera] No PlayerInputHandler instance found.");
            enabled = false;
            return;
        }

        if (playerBody == null)
        {
            Debug.LogError("[PlayerCamera] Missing playerBody reference.");
            enabled = false;
            return;
        }

        _xRotation = NormalizeAngle(transform.localEulerAngles.x);

        if (GameSettings.Instance != null)
        {
            _sensitivity = GameSettings.Instance.mouseSensitivity;
            GameSettings.Instance.OnSensitivityChanged += OnSensitivityChanged;
        }
        else
        {
            _sensitivity = 2f;
        }

        LockCursor(true);
    }

    private void OnDestroy()
    {
        if (GameSettings.Instance != null)
            GameSettings.Instance.OnSensitivityChanged -= OnSensitivityChanged;
    }

    private void Update()
    {
        UpdateZoom();

        if (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;
        if (IsLocked) return;
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 targetLook = _input.LookInput * (_sensitivity * lookInputMultiplier);

        if (smoothTime > 0f)
        {
            _currentLook = Vector2.SmoothDamp(
                _currentLook,
                targetLook,
                ref _lookVelocity,
                smoothTime
            );
        }
        else
        {
            _currentLook = targetLook;
        }

        _xRotation -= _currentLook.y;
        _xRotation = Mathf.Clamp(_xRotation, minVerticalAngle, maxVerticalAngle);

        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * _currentLook.x);
    }

    private void UpdateZoom()
    {
        if (vcam == null) return;
        LensSettings lens = vcam.Lens;
        float targetFOV = _isAiming ? aimFOV : _baseFOV;
        lens.FieldOfView = Mathf.Lerp(lens.FieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
        vcam.Lens = lens;
    }

    public static void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    private void OnSensitivityChanged(float newValue)
    {
        _sensitivity = newValue;
    }

    private static float NormalizeAngle(float angle)
    {
        if (angle > 180f)
            angle -= 360f;

        return angle;
    }
}