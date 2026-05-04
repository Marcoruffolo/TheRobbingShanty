using UnityEngine;
using UnityEngine.Events;

public class ShipRepairInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private ShipRepairData repairData;

    public string InteractionPrompt => "Repair Ship";
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public static UnityAction<ShipRepairData> OnRepairUIOpened;
    public static UnityAction OnRepairUIClosed;

    private Interactor _interactor;

    private void OnEnable()  => ShipRepairUI.OnCloseRequested += HandleCloseRequested;
    private void OnDisable() => ShipRepairUI.OnCloseRequested -= HandleCloseRequested;

    private void HandleCloseRequested() => _interactor?.RequestEndInteraction();

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        _interactor = interactor;
        Debug.Log($"[ShipRepair] Interact called. repairData={repairData}, listeners={OnRepairUIOpened?.GetInvocationList().Length ?? 0}");
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
