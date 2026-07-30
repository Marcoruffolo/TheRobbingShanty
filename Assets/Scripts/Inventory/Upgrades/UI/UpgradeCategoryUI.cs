using UnityEngine;

public class UpgradeCategoryUI : MonoBehaviour, IGameEventListener<bool>
{
    [SerializeField] private ChalkBoardStation station;
    [SerializeField] private BoolGameEvent onUpgradeUIOpen;
    [SerializeField] private BoolGameEvent onItemUpgradeUIOpen;
    [SerializeField] private BoolGameEvent onShipUpgradeUIOpen;
    [SerializeField] private GameObject panel;

    private void Awake() => panel.SetActive(false);

    private void OnEnable() => onUpgradeUIOpen.RegisterListener(this);

    private void OnDisable() => onUpgradeUIOpen.UnregisterListener(this);

    // Station opened -> show category picker. Station closed -> hide it too.
    public void OnEventRaised(bool isOpen) => panel.SetActive(isOpen);

    public void OnWeaponUpgradesButtonPressed()
    {
        panel.SetActive(false);
        onItemUpgradeUIOpen.Raise(true);
    }

    public void OnShipUpgradesButtonPressed()
    {
        panel.SetActive(false);
        onShipUpgradeUIOpen.Raise(true);
    }

    public void OnCloseButtonPressed() => station?.EndInteraction();
}