using UnityEngine;
using UnityEngine.Events;

public class ShipRepairInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private ShipRepairData repairData;
    [SerializeField] private SOVariableInt shipRepairLevel;
    [SerializeField] private int repairLevel = 2;

    [Header("Prompts")]
    [SerializeField] private string repairPrompt = "Repair Ship";
    [SerializeField] private string helmPrompt = "Take Wheel";

    public string InteractionPrompt => IsRepaired ? helmPrompt : repairPrompt;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public static UnityAction<ShipRepairData> OnRepairUIOpened;
    public static UnityAction OnRepairUIClosed;

    private Interactor _interactor;
    private bool IsRepaired => shipRepairLevel != null && shipRepairLevel.Value >= repairLevel;

    private void OnEnable()
    {
        ShipRepairUI.OnCloseRequested += HandleCloseRequested;
        ShipRepairUI.OnRepairCompleted += HandleRepairCompleted;
    }

    private void OnDisable()
    {
        ShipRepairUI.OnCloseRequested -= HandleCloseRequested;
        ShipRepairUI.OnRepairCompleted -= HandleRepairCompleted;
    }

    private void HandleCloseRequested() => _interactor?.RequestEndInteraction();
    private void HandleRepairCompleted() => shipRepairLevel?.Add(1);

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        if (IsRepaired)
        {
            interactSuccessful = false;
            return;
        }

        _interactor = interactor;
        PlayerCamera.LockCursor(false);
        BlockPlayerMovement.Instance?.ImmobilizePlayer();
        OnRepairUIOpened?.Invoke(repairData);
        interactSuccessful = true;
    }

    public void Interact() => Interact(null, out _);

    public void EndInteraction()
    {
        OnRepairUIClosed?.Invoke();
        PlayerCamera.LockCursor(true);
        BlockPlayerMovement.Instance?.FreePlayer();
        OnInteractionComplete?.Invoke(this);
    }
}
