using UnityEngine;

public class PlayerTeleport : MonoBehaviour
{
    [Header("Teleport")]
    [SerializeField] private Transform destination;
    [SerializeField] private string promptMessage = "[R] Teleport";

    [Header("HUD")]
    [SerializeField] private BoolGameEvent showPromptEvent;
    [SerializeField] private SOVariableString promptText;

    private PlayerMovement _playerMovement;
    private PlayerSwimming _swimming;
    private PlayerInputHandler _input;
    private bool _subscribed;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _swimming = GetComponent<PlayerSwimming>();
    }

    private void OnEnable()
    {
        PlayerSwimming.OnSwimStateChanged += HandleSwimStateChanged;
        TrySubscribeToInput();
    }

    private void Start()
    {
        TrySubscribeToInput();
        HandleSwimStateChanged(_swimming != null && _swimming.IsSwimming);
    }

    private void OnDisable()
    {
        PlayerSwimming.OnSwimStateChanged -= HandleSwimStateChanged;
        UnsubscribeFromInput();

        if (_swimming != null && _swimming.IsSwimming)
            SetPrompt("", false);
    }

    private void TrySubscribeToInput()
    {
        if (_subscribed) return;

        if (_input == null)
            _input = PlayerInputHandler.Instance;

        if (_input == null) return;

        _input.OnTeleport += HandleTeleportPressed;
        _subscribed = true;
    }

    private void UnsubscribeFromInput()
    {
        if (!_subscribed || _input == null) return;

        _input.OnTeleport -= HandleTeleportPressed;
        _subscribed = false;
    }

    private void HandleSwimStateChanged(bool isSwimming)
    {
        SetPrompt(isSwimming ? promptMessage : "", isSwimming);
    }

    private void HandleTeleportPressed()
    {
        if (_swimming == null || !_swimming.IsSwimming)
            return;

        TeleportToDestination();
    }

    private void TeleportToDestination()
    {
        if (destination == null)
        {
            Debug.LogWarning("[PlayerTeleport] Missing destination reference.");
            return;
        }

        _swimming.ExitWater();
        _playerMovement.TeleportTo(destination.position, destination.rotation);
    }

    private void SetPrompt(string text, bool visible)
    {
        if (promptText != null) promptText.Value = text;
        showPromptEvent?.Raise(visible);
    }
}
