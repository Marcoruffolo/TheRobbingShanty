using UnityEngine;
using Unity.Cinemachine;

public class Telescope : MonoBehaviour, IInteractable
{
    [Header("Cinemachine")]
    [SerializeField] private CinemachineCamera telescopeVCam;
    [SerializeField] private Camera telescopeRenderCam;
    [SerializeField] private TelescopeCamera telescopeCam;

    [Header("Zoom")]
    [SerializeField] private SOVariableFloat TelescopeZoom;

    [Header("HUD")]
    [SerializeField] private GameObject telescopeOverlay;

    private bool _isActive;

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
        InitVCam();

        SetOverlayActive(false);
        SetRenderCamActive(false);
    }

    private void OnDisable()
    {
        if (_isActive)
            SetTelescopeActive(false);
    }

    private void ValidateReferences()
    {
        if (telescopeVCam == null)
            Debug.LogError("[Telescope] Falta asignar telescopeVCam.", this);
        if (telescopeRenderCam == null)
            Debug.LogError("[Telescope] Falta asignar telescopeRenderCam.", this);
        if (telescopeOverlay == null)
            Debug.LogWarning("[Telescope] No hay telescopeOverlay asignado.", this);
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
        if (telescopeOverlay != null)
            telescopeOverlay.SetActive(active);
    }
}