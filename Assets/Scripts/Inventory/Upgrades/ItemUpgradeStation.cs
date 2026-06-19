using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemUpgradeStation : MonoBehaviour, IInteractable
{
    [SerializeField] private string interactionPrompt = "Mejorar Items";
    [SerializeField] private List<ItemUpgradeData> availableUpgrades;

    public string InteractionPrompt => interactionPrompt;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public static UnityAction OnCategoryUIOpened;
    public static UnityAction<List<ItemUpgradeData>> OnUpgradeUIOpened;
    public static UnityAction OnUpgradeUIClosed;

    private Interactor _interactor;

    private void OnEnable()
    {
        ItemUpgradeUI.OnCloseRequested += HandleCloseRequested;
        UpgradeCategoryUI.OnCloseRequested += HandleCloseRequested;
        UpgradeCategoryUI.OnWeaponUpgradesSelected += HandleWeaponUpgradesSelected;
    }

    private void OnDisable()
    {
        ItemUpgradeUI.OnCloseRequested -= HandleCloseRequested;
        UpgradeCategoryUI.OnCloseRequested -= HandleCloseRequested;
        UpgradeCategoryUI.OnWeaponUpgradesSelected -= HandleWeaponUpgradesSelected;
    }

    private void HandleCloseRequested() => _interactor?.RequestEndInteraction();
    private void HandleWeaponUpgradesSelected() => OnUpgradeUIOpened?.Invoke(availableUpgrades);

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        _interactor = interactor;
        PlayerCamera.LockCursor(false);
        BlockPlayerMovement.Instance?.ImmobilizePlayer();
        OnCategoryUIOpened?.Invoke();
        interactSuccessful = true;
    }

    public void Interact() => Interact(null, out _);

    public void EndInteraction()
    {
        OnUpgradeUIClosed?.Invoke();
        PlayerCamera.LockCursor(true);
        BlockPlayerMovement.Instance?.FreePlayer();
        OnInteractionComplete?.Invoke(this);
    }
}
