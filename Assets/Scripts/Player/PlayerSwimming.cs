using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerSwimming : MonoBehaviour
{
    private CharacterController controller;
    private PlayerMovement playerMovement;
    private PlayerInputHandler input;

    [Header("Swim Speeds")]
    [SerializeField] private float swimSpeed = 3f;
    [SerializeField] private float ascendSpeed = 2.5f;
    [SerializeField] private float descendSpeed = 2.5f;

    [Header("Detection")]
    [SerializeField] private LayerMask waterMask;

    private WaterVolume _activeWater;

    public bool IsSwimming { get; private set; }

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerMovement = GetComponent<PlayerMovement>();
    }

    private void Start()
    {
        input = PlayerInputHandler.Instance;

        if (input == null)
            Debug.LogError("[PlayerSwimming] No PlayerInputHandler instance found.");
    }

    private void OnDisable()
    {
        if (IsSwimming)
            StopSwimming();
    }

    private void Update()
    {
        if (IsSwimming)
            Swim();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsSwimming || !TryGetWaterVolume(other, out WaterVolume water) || !CanStartSwimming())
            return;

        StartSwimming(water);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!IsSwimming || other.GetComponent<WaterVolume>() != _activeWater)
            return;

        StopSwimming();
    }

    private bool CanStartSwimming()
    {
        if (input == null || playerMovement == null || !playerMovement.enabled)
            return false;

        if (BlockPlayerMovement.Instance != null && BlockPlayerMovement.Instance.IsImmobilized)
            return false;

        return true;
    }

    private bool TryGetWaterVolume(Collider other, out WaterVolume water)
    {
        water = null;

        if (((1 << other.gameObject.layer) & waterMask) == 0)
            return false;

        water = other.GetComponent<WaterVolume>();
        return water != null;
    }

    private void Swim()
    {
        Vector2 moveInput = input.MoveInput;
        Vector3 horizontalMove = (transform.right * moveInput.x + transform.forward * moveInput.y) * swimSpeed;

        float verticalSpeed = input.JumpHeld ? ascendSpeed : -descendSpeed;
        float distanceToSurface = _activeWater.SurfaceY - transform.position.y;

        if (verticalSpeed > 0f)
            verticalSpeed = Mathf.Min(verticalSpeed, Mathf.Max(0f, distanceToSurface / Time.deltaTime));

        Vector3 move = horizontalMove + Vector3.up * verticalSpeed;
        controller.Move(move * Time.deltaTime);
    }

    private void StartSwimming(WaterVolume water)
    {
        _activeWater = water;
        IsSwimming = true;

        playerMovement.ResetMotionState();
        playerMovement.enabled = false;
    }

    private void StopSwimming()
    {
        IsSwimming = false;
        _activeWater = null;

        playerMovement.ResetMotionState();
        playerMovement.enabled = true;
    }
}
