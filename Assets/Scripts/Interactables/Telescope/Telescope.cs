using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;

public class Telescope : MonoBehaviour, IInteractable
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera telescopeVCam;
    [SerializeField] private Camera telescopeRenderCam;
    [SerializeField] private TelescopeCamera telescopeCam;

    [Header("Zoom")]
    [SerializeField] private SOVariableFloat TelescopeZoom;
    [SerializeField] private float zoomMin = 5f;
    [SerializeField] private float zoomMax = 60f;
    [SerializeField] private float zoomSpeed = 5f;
    [SerializeField] private InputActionReference scrollAction;

    public ScriptableRendererFeature _telescopefullscreen;

    private bool _isActive;
    private float _currentFov;
    public string InteractionPrompt => _isActive ? "Soltar catalejo" : "Usar catalejo";
    public UnityEngine.Events.UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public void Interact() => Interact(null, out _);

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        interactSuccessful = true;
        SetTelescopeActive(!_isActive);

        if (_isActive)
            BlockPlayerMovement.Instance?.ImmobilizePlayer();
        else
            BlockPlayerMovement.Instance?.FreePlayer();
    }

    public void EndInteraction()
    {
        if (_isActive)
            SetTelescopeActive(false);
    }

    private void Start()
    {
        telescopeCam = GetComponentInChildren<TelescopeCamera>();
        ValidateReferences();
        _currentFov = TelescopeZoom.Value;
        InitVCam();
        SetOverlayActive(false);
        SetRenderCamActive(false);
    }

    private void ApplyFov(float fov)
    {
        if (telescopeVCam == null) return;
        var lens = telescopeVCam.Lens;
        lens.FieldOfView = fov;
        telescopeVCam.Lens = lens;
    }
    private void OnEnable()
    {
        scrollAction.action.performed += OnScroll;
    }

    private void OnDisable()
    {
        scrollAction.action.performed -= OnScroll;
        if (_isActive)
            SetTelescopeActive(false);
    }

    private void OnScroll(InputAction.CallbackContext ctx)
    {
        if (!_isActive) return;

        float scroll = ctx.ReadValue<Vector2>().y;
        float normalizedScroll = scroll / 120f;

        _currentFov -= normalizedScroll * zoomSpeed;
        _currentFov = Mathf.Clamp(_currentFov, zoomMin, zoomMax);

        ApplyFov(_currentFov);
        TelescopeZoom.Value = _currentFov;
    }


    private void ValidateReferences()
    {
        if (telescopeVCam == null)
            Debug.LogError("[Telescope] Falta asignar telescopeVCam.", this);
        if (telescopeRenderCam == null)
            Debug.LogError("[Telescope] Falta asignar telescopeRenderCam.", this);
    }

    private void InitVCam()
    {
        if (telescopeVCam == null) return;

        var lens = telescopeVCam.Lens;
        lens.FieldOfView = TelescopeZoom.Value;
        telescopeVCam.Lens = lens;
    }

    private void SetTelescopeActive(bool active)
    {
        _isActive = active;
        telescopeCam.IsActive = active;

        SetRenderCamActive(active);
        SetOverlayActive(active);

    }

    private void SetRenderCamActive(bool active)
    {
        if (telescopeRenderCam != null)
            telescopeRenderCam.gameObject.SetActive(active);
    }

    private void SetOverlayActive(bool active)
    {
        _telescopefullscreen.SetActive(active);
    }
}