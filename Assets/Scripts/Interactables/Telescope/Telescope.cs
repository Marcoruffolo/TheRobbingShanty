using UnityEngine;
using UnityEngine.Events;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class TelescopeInteractable : MonoBehaviour, IInteractable
{
    [field: SerializeField] public string InteractionPrompt { get; private set; } = "Usar telescopio";
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera telescopeCam;

    [Header("Pan / Tilt")]
    [SerializeField] private float panSpeed = 10f;
    [SerializeField] private float tiltSpeed = 10f;
    [SerializeField] private float minTilt = -20f;
    [SerializeField] private float maxTilt = 35f;
    [SerializeField] private float minPan = -80f;
    [SerializeField] private float maxPan = 80f;

    [Header("Zoom")]
    [SerializeField] private float minFOV = 5f;
    [SerializeField] private float maxFOV = 30f;
    [SerializeField] private float defaultFOV = 20f;
    [SerializeField] private float zoomStep = 2f;
    [SerializeField] private float zoomSmoothing = 8f;

    [Header("HUD")]
    [SerializeField] private GameObject telescopeHUD;

    private bool _isInUse;
    private float _targetFOV;
    private float _currentFOV;

    private CinemachinePanTilt _panTilt;

    private void Awake()
    {
        if (telescopeCam != null)
        {
            telescopeCam.gameObject.SetActive(false);
            _panTilt = telescopeCam.GetComponent<CinemachinePanTilt>();
            if (_panTilt == null)
                Debug.LogWarning("[Telescope] CinemachinePanTilt no encontrado.");
        }

        if (telescopeHUD != null) telescopeHUD.SetActive(false);

        _targetFOV = defaultFOV;
        _currentFOV = defaultFOV;

    }

    private void Start()
    {

        if (PlayerInputHandler.Instance != null)
            PlayerInputHandler.Instance.OnInteract += HandleInteractInput;
    }

    private void OnDestroy()
    {
        if (PlayerInputHandler.Instance != null)
            PlayerInputHandler.Instance.OnInteract -= HandleInteractInput;

    }
    private void Update()
    {
        if (!_isInUse) return;

        HandlePanTilt();
        HandleZoom();

        _currentFOV = zoomSmoothing > 0f
            ? Mathf.Lerp(_currentFOV, _targetFOV, Time.deltaTime * zoomSmoothing)
            : _targetFOV;

        // Reemplazá la línea anterior por esto:
        if (telescopeCam != null)
        {
            var lens = telescopeCam.Lens;
            lens.FieldOfView = _currentFOV;
            telescopeCam.Lens = lens;
            var brain = CinemachineBrain.GetActiveBrain(0);

            Debug.Log($"ActiveVCam: {brain?.ActiveVirtualCamera?.Name} | MainCamFOV: {brain?.OutputCamera?.fieldOfView} | LensFOV: {telescopeCam.Lens.FieldOfView}");

        }
    }

    private void HandleZoom()
    {
        if (Mouse.current == null) return;
        float scrollY = Mouse.current.scroll.ReadValue().y;
        Debug.Log($"Scroll raw: {scrollY}");
        if (Mathf.Approximately(scrollY, 0f)) return;
        _targetFOV = Mathf.Clamp(_targetFOV - Mathf.Sign(scrollY) * zoomStep, minFOV, maxFOV);
        Debug.Log($"TargetFOV: {_targetFOV}");
    }

    // Orden de ejecución real del frame:
    // 1. Update          → calculamos _currentFOV
    // 2. LateUpdate      → (no usamos)
    // 3. Camera.onPreCull → Cinemachine Brain aplica su FOV (nos pisaba)
    // 4. Camera.onPreRender → NOSOTROS sobreescribimos aquí ← la solución
    // 5. Render del frame

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        interactSuccessful = !_isInUse;
        if (interactSuccessful) EnterTelescope();
    }

    public void Interact()
    {
        if (!_isInUse) EnterTelescope();
        else ExitTelescope();
    }

    public void EndInteraction()
    {
        if (_isInUse) ExitTelescope();
    }

    private void EnterTelescope()
    {
        _isInUse = true;

        _targetFOV = defaultFOV;
        _currentFOV = defaultFOV;

        if (telescopeCam != null)
            telescopeCam.gameObject.SetActive(true);

        if (_panTilt != null)
        {
            var pan = _panTilt.PanAxis; pan.Value = 0f; _panTilt.PanAxis = pan;
            var tilt = _panTilt.TiltAxis; tilt.Value = 0f; _panTilt.TiltAxis = tilt;
        }

        if (telescopeHUD != null) telescopeHUD.SetActive(true);

        if (BlockPlayerMovement.Instance != null)
            BlockPlayerMovement.Instance.ImmobilizePlayer();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (TelescopeRendererFeature.Instance != null)
            TelescopeRendererFeature.Instance.IsActive = true;
        else
            Debug.LogError("TelescopeRendererFeature.Instance es null!");
    }

    private void ExitTelescope()
    {
        _isInUse = false;
        if (telescopeCam != null) telescopeCam.gameObject.SetActive(false);
        if (telescopeHUD != null) telescopeHUD.SetActive(false);

        if (BlockPlayerMovement.Instance != null)
            BlockPlayerMovement.Instance.FreePlayer();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        OnInteractionComplete?.Invoke(this);

        if (TelescopeRendererFeature.Instance != null)
            TelescopeRendererFeature.Instance.IsActive = false;
    }

    private void HandleInteractInput()
    {
        if (_isInUse) ExitTelescope();
    }

    private void HandlePanTilt()
    {
        if (_panTilt == null || PlayerInputHandler.Instance == null) return;

        Vector2 look = PlayerInputHandler.Instance.LookInput;

        InputAxis pan = _panTilt.PanAxis;
        pan.Value = Mathf.Clamp(pan.Value + look.x * panSpeed * Time.deltaTime, minPan, maxPan);
        _panTilt.PanAxis = pan;

        InputAxis tilt = _panTilt.TiltAxis;
        tilt.Value = Mathf.Clamp(tilt.Value - look.y * tiltSpeed * Time.deltaTime, minTilt, maxTilt);
        _panTilt.TiltAxis = tilt;
    }
}