using UnityEngine;
using Unity.Cinemachine;

public class BlockPlayerMovement : MonoBehaviour
{
    public static BlockPlayerMovement Instance { get; private set; }

    [SerializeField] private bool blockMovement = true;
    [SerializeField] private bool blockCamera = true;

    private PlayerMovement _movement;
    private PlayerCamera _camera;
    private CinemachineCamera _vcam;  // ← tipo correcto

    public bool IsImmobilized { get; private set; }

    public event System.Action OnImmobilized;
    public event System.Action OnFreed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;

        _movement = GetComponent<PlayerMovement>();
        _camera = GetComponentInChildren<PlayerCamera>();
        _vcam = GetComponentInChildren<CinemachineCamera>();
    }

    public void ImmobilizePlayer()
    {
        if (IsImmobilized) return;
        IsImmobilized = true;
        SetSystems(false);
        OnImmobilized?.Invoke();
    }

    public void FreePlayer()
    {
        if (!IsImmobilized) return;
        IsImmobilized = false;
        SetSystems(true);
        OnFreed?.Invoke();
    }

    private void SetSystems(bool state)
    {
        if (blockMovement && _movement != null) _movement.enabled = state;
        if (blockCamera && _camera != null) _camera.IsLocked = !state;

        if (blockMovement && _movement != null) _movement.enabled = state;
        if (blockCamera && _camera != null) _camera.IsLocked = !state;
    }
}