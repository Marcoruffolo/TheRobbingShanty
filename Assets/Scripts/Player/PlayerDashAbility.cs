using UnityEngine;
using System.Collections;
public class PlayerDashAbility : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerInputHandler input;

    [Header("Dashing")]
    [SerializeField] private float dashDistance = 4f;
    [SerializeField] private float dashDuration = 0.18f;

    [Header("Settings")]
    [SerializeField] private bool useCameraForward = true;
    [SerializeField] private bool allowAllDirections = true;
    [SerializeField] private bool allowAirDash;

    [Header("Cooldown")]
    [SerializeField] private float cooldown = 1f;

    private Camera _camera;
    private Coroutine _dashCoroutine;
    private bool _subscribed;
    private float _nextDashTime;

    private void Awake()
    {
        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (input == null)
            input = GetComponent<PlayerInputHandler>();
    }

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();

        if (_dashCoroutine != null)
            StopCoroutine(_dashCoroutine);

        _dashCoroutine = null;
        playerMovement?.SetDashLocked(false);
    }

    private void TrySubscribe()
    {
        if (_subscribed) return;

        if (input == null)
            input = PlayerInputHandler.Instance;

        if (input == null) return;

        input.OnDash += TryDash;
        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed || input == null) return;

        input.OnDash -= TryDash;
        _subscribed = false;
    }

    private void TryDash()
    {
        if (!CanDash()) return;

        if (!TryGetDashDirection(out Vector3 direction)) return;

        _nextDashTime = Time.time + cooldown;
        _dashCoroutine = StartCoroutine(Dash(direction));
    }

    private bool CanDash()
    {
        if (_dashCoroutine != null || Time.time < _nextDashTime) return false;
        if (controller == null || !controller.enabled) return false;
        if (playerMovement == null || !playerMovement.enabled) return false;
        if (!allowAirDash && !playerMovement.IsGrounded) return false;

        if (BlockPlayerMovement.Instance != null && BlockPlayerMovement.Instance.IsImmobilized)
            return false;

        PlayerSurfaceClimber climber = GetComponent<PlayerSurfaceClimber>();
        return climber == null || !climber.IsClimbing;
    }

    private bool TryGetDashDirection(out Vector3 direction)
    {
        Transform forwardTransform = GetForwardTransform();
        Vector3 forward = forwardTransform.forward;
        Vector3 right = forwardTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector2 moveInput = input != null ? input.MoveInput : Vector2.zero;
        if (allowAllDirections && moveInput.sqrMagnitude > 0.0001f)
            direction = forward * moveInput.y + right * moveInput.x;
        else
            direction = forward;

        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.0001f)
            return false;

        direction.Normalize();
        return true;
    }

    private Transform GetForwardTransform()
    {
        if (useCameraForward)
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_camera != null)
                return _camera.transform;
        }

        return transform;
    }

    private IEnumerator Dash(Vector3 direction)
    {
        playerMovement.SetDashLocked(true);

        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, dashDuration);
        float speed = dashDistance / duration;

        while (elapsed < duration)
        {
            float deltaTime = Time.deltaTime;
            controller.Move(direction * speed * deltaTime);
            elapsed += deltaTime;
            yield return null;
        }

        playerMovement.SetDashLocked(false);
        _dashCoroutine = null;
    }
}
