using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speeds")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -19.62f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundRadius = 0.3f;
    [SerializeField] private LayerMask groundMask;

    private CharacterController _controller;
    private PlayerInputHandler _input;
    private Vector3 _velocity;
    private bool _isGrounded;
    private bool _jumpRequested;

    public bool IsGrounded => _isGrounded;
    public bool IsDashLocked { get; private set; }

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        _input = PlayerInputHandler.Instance;

        if (_input == null)
        {
            Debug.LogError("[PlayerMovement] No PlayerInputHandler instance found.");
            return;
        }

        _input.OnJump += RequestJump;
    }

    private void OnDestroy()
    {
        if (_input != null)
            _input.OnJump -= RequestJump;
    }

    private void Update()
    {
        Shader.SetGlobalVector("_Player", transform.position);
        CheckGround();
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    public void ResetMotionState()
    {
        _velocity = Vector3.zero;
        _isGrounded = false;
        _jumpRequested = false;
    }

    public void SetDashLocked(bool locked)
    {
        IsDashLocked = locked;
    }

    public void TeleportTo(Vector3 position, Quaternion rotation)
    {
        ResetMotionState();

        bool controllerWasEnabled = _controller.enabled;
        _controller.enabled = false;

        transform.SetPositionAndRotation(position, rotation);

        _controller.enabled = controllerWasEnabled;
    }

    private void RequestJump()
    {
        if (_isGrounded)
            _jumpRequested = true;
    }

    private void CheckGround()
    {
        _isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundRadius,
            groundMask,
            QueryTriggerInteraction.Ignore
        );

        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
    }

    private void HandleMovement()
    {
        if (_input == null || IsDashLocked)
            return;

        float speed = _input.SprintHeld ? sprintSpeed : walkSpeed;
        Vector2 input = _input.MoveInput;
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        _controller.Move(move * speed * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (_jumpRequested && _isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _jumpRequested = false;
        }
        else if (!_isGrounded)
        {
            _jumpRequested = false;
        }
    }

    private void ApplyGravity()
    {
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
            return;

        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}