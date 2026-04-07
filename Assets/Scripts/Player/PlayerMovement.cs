using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Velocidades")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float jumpHeight = 1.2f;

    [Header("Gravedad")]
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

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        _input = PlayerInputHandler.Instance;

        if (_input == null)
        {
            Debug.LogError("[PlayerMovement] No se encontró PlayerInputHandler.");
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
        // ORDEN CRÍTICO: ground check primero, luego jump, luego gravity
        CheckGround();
        HandleMovement();
        HandleJump();
        ApplyGravity();
    }

    private void RequestJump()
    {
        // Solo registrar el pedido si estamos en el suelo en este momento
        if (_isGrounded)
            _jumpRequested = true;
    }

    private void CheckGround()
    {
        _isGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundRadius,
            groundMask,
            QueryTriggerInteraction.Ignore   // ← ignora triggers, evita falsos positivos
        );

        if (_isGrounded && _velocity.y < 0f)
            _velocity.y = -2f;
    }

    private void HandleMovement()
    {
        if (_input == null) return;

        float speed = _input.SprintHeld ? sprintSpeed : walkSpeed;
        Vector2 input = _input.MoveInput;
        Vector3 move = transform.right * input.x + transform.forward * input.y;

        _controller.Move(move * speed * Time.deltaTime);
    }

    private void HandleJump()
    {
        if (_jumpRequested && _isGrounded)  // doble check: evento + estado actual
        {
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            _jumpRequested = false;
        }
        else if (!_isGrounded)
        {
            // Limpiar el pedido si llegó tarde (cayó del borde mientras presionaba)
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
        if (groundCheck == null) return;
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}