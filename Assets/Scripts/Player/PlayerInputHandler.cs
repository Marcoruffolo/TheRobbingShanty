using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance { get; private set; }

    [Header("Input Actions Asset")]
    [SerializeField] private InputActionAsset inputActions;

    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _sprintAction;
    private InputAction _interactAction;
    private InputAction _attackAction;
    private InputAction _parryAction;
    private InputAction _pauseAction;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool SprintHeld { get; private set; }

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

        if (inputActions == null)
        {
            Debug.LogError("[PlayerInputHandler] Missing InputActionAsset reference.");
            enabled = false;
            return;
        }

        InputActionMap map = inputActions.FindActionMap("Player", true);

        _moveAction = map.FindAction("Move", true);
        _lookAction = map.FindAction("Look", true);
        _jumpAction = map.FindAction("JumpAction", true);
        _sprintAction = map.FindAction("Sprint", true);
        _interactAction = map.FindAction("InteractAction", true);
        _attackAction = map.FindAction("AttackAction", true);
        _parryAction = map.FindAction("ParryAction", true);
        _pauseAction = map.FindAction("Pause", true);
    }

    private void OnEnable()
    {
        if (inputActions == null)
            return;

        inputActions.Enable();

        _jumpAction.performed += HandleJumpPerformed;
        _interactAction.performed += HandleInteractPerformed;
        _attackAction.performed += HandleAttackPerformed;
        _parryAction.performed += HandleParryPerformed;
        _pauseAction.performed += HandlePausePerformed;
    }

    private void OnDisable()
    {
        if (inputActions == null)
            return;

        _jumpAction.performed -= HandleJumpPerformed;
        _interactAction.performed -= HandleInteractPerformed;
        _attackAction.performed -= HandleAttackPerformed;
        _parryAction.performed -= HandleParryPerformed;
        _pauseAction.performed -= HandlePausePerformed;

        inputActions.Disable();

        MoveInput = Vector2.zero;
        LookInput = Vector2.zero;
        SprintHeld = false;
    }

    private void Update()
    {
        MoveInput = _moveAction.ReadValue<Vector2>();
        LookInput = _lookAction.ReadValue<Vector2>();
        SprintHeld = _sprintAction.IsPressed();
    }

    private void HandleJumpPerformed(InputAction.CallbackContext context)
    {
        OnJump?.Invoke();
    }

    private void HandleInteractPerformed(InputAction.CallbackContext context)
    {
        OnInteract?.Invoke();
    }

    private void HandleAttackPerformed(InputAction.CallbackContext context)
    {
        OnAttack?.Invoke();
    }

    private void HandleParryPerformed(InputAction.CallbackContext context)
    {
        OnParry?.Invoke();
    }

    private void HandlePausePerformed(InputAction.CallbackContext context)
    {
        OnPauseToggle?.Invoke();
    }
}