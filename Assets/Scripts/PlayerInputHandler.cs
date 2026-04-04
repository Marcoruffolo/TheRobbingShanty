using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Centraliza todo el input del jugador usando el New Input System.
/// Usa ReadValue y WasPressedThisFrame directamente sobre el InputActionAsset,
/// lo que es más confiable que Send Messages para detectar estados.
///
/// Setup:
/// - Asigná el archivo .inputactions en el campo "Input Actions Asset"
/// - El componente PlayerInput del mismo GO puede quedar en Behavior: Invoke Unity Events
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance { get; private set; }

    [Header("Input Actions Asset")]
    [SerializeField] private InputActionAsset inputActions;

    // Referencias a las actions 
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _interactAction;
    private InputAction _attackAction;
    private InputAction _parryAction;
    private InputAction _pauseAction;

    // Valores públicos leídos cada frame 
    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool SprintHeld { get; private set; }

    // Eventos para acciones puntuales 
    public event System.Action OnJump;
    public event System.Action OnInteract;
    public event System.Action OnAttack;
    public event System.Action OnParry;
    public event System.Action OnPauseToggle;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        var map = inputActions.FindActionMap("Player", throwIfNotFound: true);

        _moveAction = map.FindAction("Move", throwIfNotFound: true);
        _lookAction = map.FindAction("Look", throwIfNotFound: true);
        _jumpAction = map.FindAction("JumpAction", throwIfNotFound: true);
        _sprintAction = map.FindAction("Sprint", throwIfNotFound: true);
        _interactAction = map.FindAction("InteractAction", throwIfNotFound: true);
        _attackAction = map.FindAction("AttackAction", throwIfNotFound: true);
        _parryAction = map.FindAction("ParryAction", throwIfNotFound: true);
        _pauseAction = map.FindAction("Pause", throwIfNotFound: true);
    }

    private void OnEnable()
    {
        inputActions.Enable();

        _jumpAction.performed += _ => OnJump?.Invoke();
        _interactAction.performed += _ => OnInteract?.Invoke();
        _attackAction.performed += _ => OnAttack?.Invoke();
        _parryAction.performed += _ => OnParry?.Invoke();
        _pauseAction.performed += _ => OnPauseToggle?.Invoke();
    }

    private void OnDisable()
    {
        inputActions.Disable();

        _jumpAction.performed -= _ => OnJump?.Invoke();
        _interactAction.performed -= _ => OnInteract?.Invoke();
        _attackAction.performed -= _ => OnAttack?.Invoke();
        _parryAction.performed -= _ => OnParry?.Invoke();
        _pauseAction.performed -= _ => OnPauseToggle?.Invoke();
    }

    private void Update()
    {
        MoveInput = _moveAction.ReadValue<Vector2>();
        LookInput = _lookAction.ReadValue<Vector2>();
        SprintHeld = _sprintAction.IsPressed();
    }
}