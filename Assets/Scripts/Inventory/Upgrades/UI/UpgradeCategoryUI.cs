using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UpgradeCategoryUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    public static UnityAction OnWeaponUpgradesSelected;
    public static UnityAction OnCloseRequested;

    private void Awake() => panel.SetActive(false);

    private void OnEnable()
    {
        ItemUpgradeStation.OnCategoryUIOpened += Open;
        ItemUpgradeStation.OnUpgradeUIClosed += Close;
        ItemUpgradeStation.OnUpgradeUIOpened += HandleWeaponUIOpened;
    }

    private void OnDisable()
    {
        ItemUpgradeStation.OnCategoryUIOpened -= Open;
        ItemUpgradeStation.OnUpgradeUIClosed -= Close;
        ItemUpgradeStation.OnUpgradeUIOpened -= HandleWeaponUIOpened;
    }

    private void Open() => panel.SetActive(true);

    private void Close() => panel.SetActive(false);

    private void HandleWeaponUIOpened(List<ItemUpgradeData> _) => panel.SetActive(false);

    public void OnWeaponUpgradesButtonPressed() => OnWeaponUpgradesSelected?.Invoke();

    public void OnCloseButtonPressed() => OnCloseRequested?.Invoke();
}
