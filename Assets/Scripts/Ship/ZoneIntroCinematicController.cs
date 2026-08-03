using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering.Universal;

public class ZoneIntroCinematicController : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private NavigationRunState navigationRunState;
    [SerializeField] private int zoneIndex;
    [SerializeField] private bool playOncePerRun = true;
    [SerializeField] private float startDelay = 0.35f;
    [SerializeField] private float targetWaitTimeout = 4f;
    [SerializeField] private float telescopeEffectEndTime = 3.6f;
    [SerializeField] private bool holdLoadingOverlayUntilCinematic = true;

    [Header("Timeline")]
    [SerializeField] private PlayableDirector cine;
    [SerializeField] private CinemachineCamera doorCamera;
    [SerializeField] private CinemachineCamera tableCamera;
    [SerializeField] private int cinematicCameraPriority = 100;
    [SerializeField] private int inactiveCameraPriority = 0;

    [Header("Targets")]
    [SerializeField] private Transform doorTarget;
    [SerializeField] private Transform tableTarget;
    [SerializeField] private Vector3 doorCameraOffset = new Vector3(0f, 20f, -70f);
    [SerializeField] private Vector3 doorLookOffset = new Vector3(0f, 6f, 0f);
    [SerializeField] private Vector3 tableCameraOffset = new Vector3(0f, 2.2f, -2.15f);
    [SerializeField] private Vector3 tableLookOffset = new Vector3(0f, 0.35f, 0f);

    [Header("Shake")]
    [SerializeField] private Transform shakeTarget;
    [SerializeField] private float shakeStartTime = 2.7f;
    [SerializeField] private float shakeEndTime = 5.2f;
    [SerializeField] private float shakeAmplitude = 0.015f;
    [SerializeField] private float shakeFrequency = 22f;

    [Header("Telescope / HUD")]
    [SerializeField] private ScriptableRendererFeature telescopeEffect;
    [SerializeField] private BoolGameEvent showPromptEvent;
    [SerializeField] private SOVariableString promptText;
    [SerializeField] private string introMessage = "La puerta esta sellada. Vincula nucleos en la mesa del barco.";

    private bool _cinematicEnded;
    private bool _isPlaying;
    private Vector3 _shakeBaseLocalPosition;
    private bool _telescopeEffectDisabledDuringPlayback;
    private bool _holdingLoadingOverlay;

    private NavigationRunState State => navigationRunState != null ? navigationRunState : NavigationRunState.Instance;

    private void Awake()
    {
        if (!holdLoadingOverlayUntilCinematic || SceneLoader.Instance == null) return;

        SceneLoader.Instance.HoldBlackScreen(false);
        _holdingLoadingOverlay = true;
    }
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(startDelay);

        if (playOncePerRun && State.HasIntroPlayed(zoneIndex))
        {
            ReleaseLoadingOverlay();
            yield break;
        }

        if (cine == null)
        {
            Debug.LogError("[ZoneIntroCinematic] Falta PlayableDirector.", this);
            ReleaseLoadingOverlay();
            yield break;
        }

        yield return WaitForTargets();
        ConfigureCameraViews();

        if (playOncePerRun)
            State.MarkIntroPlayed(zoneIndex);

        cine.stopped += OnCinematicStopped;
        PlayCinematic();
    }

    private void Update()
    {
        if (!_isPlaying || _cinematicEnded || cine == null)
            return;

        UpdateTelescopeEffect();
        ApplyShake();

        bool isPlaying = cine.state == PlayState.Playing;
        bool reachedEnd = cine.time >= cine.duration;
        if (!isPlaying && reachedEnd)
            EndCinematic();
    }

    private void PlayCinematic()
    {
        _cinematicEnded = false;
        _isPlaying = true;
        _telescopeEffectDisabledDuringPlayback = false;

        if (shakeTarget != null)
            _shakeBaseLocalPosition = shakeTarget.localPosition;

        if (doorCamera != null)
            doorCamera.Priority = cinematicCameraPriority;
        if (tableCamera != null)
            tableCamera.Priority = cinematicCameraPriority;

        if (BlockPlayerMovement.Instance != null)
            BlockPlayerMovement.Instance.ImmobilizePlayer();

        if (telescopeEffect != null)
            telescopeEffect.SetActive(true);

        ShowMessage(true);
        cine.Play();
        ReleaseLoadingOverlay();
    }

    private void OnCinematicStopped(PlayableDirector director)
    {
        EndCinematic();
    }

    private void EndCinematic()
    {
        if (_cinematicEnded) return;
        _cinematicEnded = true;
        _isPlaying = false;

        if (shakeTarget != null)
            shakeTarget.localPosition = _shakeBaseLocalPosition;

        if (doorCamera != null)
            doorCamera.Priority = inactiveCameraPriority;
        if (tableCamera != null)
            tableCamera.Priority = inactiveCameraPriority;

        ShowMessage(false);

        if (telescopeEffect != null)
            telescopeEffect.SetActive(false);

        if (BlockPlayerMovement.Instance != null)
            BlockPlayerMovement.Instance.FreePlayer();

        ReleaseLoadingOverlay();
    }

    private void ResolveTargets()
    {
        if (doorTarget == null)
        {
            ZoneGateController gate = FindGateForZone();
            if (gate != null)
                doorTarget = gate.transform;
        }

        if (tableTarget == null)
        {
            CoreLinkingTable table = FindFirstObjectByType<CoreLinkingTable>();
            if (table != null)
                tableTarget = table.transform;
        }

        if (shakeTarget == null && tableTarget != null)
            shakeTarget = tableTarget;
    }

    private ZoneGateController FindGateForZone()
    {
        ZoneGateController[] gates = FindObjectsByType<ZoneGateController>(FindObjectsSortMode.None);
        foreach (ZoneGateController gate in gates)
        {
            if (gate.ZoneIndex == zoneIndex)
                return gate;
        }

        return null;
    }

    private IEnumerator WaitForTargets()
    {
        float elapsed = 0f;
        while (elapsed < targetWaitTimeout)
        {
            ResolveTargets();
            if (doorTarget != null && tableTarget != null)
                yield break;

            elapsed += Time.deltaTime;
            yield return null;
        }

        ResolveTargets();

        if (doorTarget == null)
            Debug.LogWarning("[ZoneIntroCinematic] No se encontro ZoneGateController para apuntar a la puerta.", this);
        if (tableTarget == null)
            Debug.LogWarning("[ZoneIntroCinematic] No se encontro CoreLinkingTable para apuntar a la mesa.", this);
    }
    private void ConfigureCameraViews()
    {
        ConfigureCamera(doorCamera, doorTarget, doorCameraOffset, doorLookOffset, false);
        ConfigureCamera(tableCamera, tableTarget, tableCameraOffset, tableLookOffset, true);
    }

    private void ConfigureCamera(CinemachineCamera cam, Transform target, Vector3 localOffset, Vector3 localLookOffset, bool useTargetSpace)
    {
        if (cam == null || target == null) return;

        Vector3 offset = useTargetSpace ? target.TransformDirection(localOffset) : localOffset;
        Vector3 lookOffset = useTargetSpace ? target.TransformDirection(localLookOffset) : localLookOffset;
        Vector3 lookPoint = target.position + lookOffset;

        cam.transform.position = target.position + offset;
        cam.transform.rotation = Quaternion.LookRotation(lookPoint - cam.transform.position, Vector3.up);
    }

    private void ApplyShake()
    {
        if (shakeTarget == null || cine == null) return;

        double time = cine.time;
        if (time < shakeStartTime || time > shakeEndTime)
        {
            shakeTarget.localPosition = _shakeBaseLocalPosition;
            return;
        }

        float t = (float)time * shakeFrequency;
        Vector3 shake = new Vector3(
            Mathf.Sin(t * 1.71f) * shakeAmplitude,
            Mathf.Sin(t * 2.13f) * shakeAmplitude,
            0f);

        shakeTarget.localPosition = _shakeBaseLocalPosition + shake;
    }

    private void UpdateTelescopeEffect()
    {
        if (_telescopeEffectDisabledDuringPlayback || telescopeEffect == null || cine == null)
            return;

        if (cine.time >= telescopeEffectEndTime)
        {
            telescopeEffect.SetActive(false);
            _telescopeEffectDisabledDuringPlayback = true;
        }
    }
    private void ReleaseLoadingOverlay()
    {
        if (!_holdingLoadingOverlay || SceneLoader.Instance == null) return;

        SceneLoader.Instance.ReleaseBlackScreen();
        _holdingLoadingOverlay = false;
    }
    private void ShowMessage(bool visible)
    {
        if (promptText != null && visible)
            promptText.Value = introMessage;

        showPromptEvent?.Raise(visible);
    }

    private void OnDestroy()
    {
        if (cine != null)
            cine.stopped -= OnCinematicStopped;

        ReleaseLoadingOverlay();
    }
}
