using UnityEngine;
using UnityEngine.Events;

public class ShipRepairInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private ShipRepairData repairData;
    [SerializeField] private SOVariableInt shipRepairLevel;
    [SerializeField] private int repairLevel = 2;

    public string InteractionPrompt => "Repair Ship";
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public static UnityAction<ShipRepairData> OnRepairUIOpened;
    public static UnityAction OnRepairUIClosed;

    private Interactor _interactor;

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
        if (shipRepairLevel != null && shipRepairLevel.Value >= repairLevel)
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
