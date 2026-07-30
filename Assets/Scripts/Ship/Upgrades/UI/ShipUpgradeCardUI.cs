using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShipUpgradeCardUI : MonoBehaviour
{
    [Header("Header")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text titleText;

    [Header("Values")]
    [SerializeField] private TMP_Text currentValueText;
    [SerializeField] private GameObject nextValueArrow;
    [SerializeField] private TMP_Text nextValueText;

    [Header("Upgrade")]
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeButtonLabel;

    [Header("Costs")]
    [SerializeField] private UpgradeCostSlotUI costSlotPrefab;
    [SerializeField] private Transform costContainer;

    private readonly List<UpgradeCostSlotUI> _costSlots = new();
    private ShipUpgradeData _data;
    private PlayerInventoryHolder _playerInventory;

    public void Setup(ShipUpgradeData data, PlayerInventoryHolder playerInventory)
    {
        _data = data;
        _playerInventory = playerInventory;

        if (icon != null && data.icon != null)
            icon.sprite = data.icon;

        if (titleText != null)
            titleText.text = data.statName;

        SubscribeInventoryEvents();
        Refresh();
    }

    private void OnEnable()
    {
        ShipUpgradeManager.OnUpgradeApplied += HandleUpgradeApplied;
    }

    private void OnDisable()
    {
        ShipUpgradeManager.OnUpgradeApplied -= HandleUpgradeApplied;
        UnsubscribeInventoryEvents();
    }

    private void SubscribeInventoryEvents()
    {
        if (_playerInventory == null) return;
        _playerInventory.PrimaryInventorySystem.OnInventorySlotChanged += HandleInventoryChanged;
        _playerInventory.SecondaryInventorySystem.OnInventorySlotChanged += HandleInventoryChanged;
    }

    private void UnsubscribeInventoryEvents()
    {
        if (_playerInventory == null) return;
        _playerInventory.PrimaryInventorySystem.OnInventorySlotChanged -= HandleInventoryChanged;
        _playerInventory.SecondaryInventorySystem.OnInventorySlotChanged -= HandleInventoryChanged;
    }

    private void HandleInventoryChanged(InventorySlot _) => Refresh();

    private void HandleUpgradeApplied(ShipUpgradeData data)
    {
        if (data == _data) Refresh();
    }

    private void Refresh()
    {
        if (_data == null) return;

        var manager = ShipUpgradeManager.Instance;
        currentValueText.text = manager.GetCurrentValue(_data).ToString("0.#");
        currentValueText.color = Color.white;

        bool hasNext = manager.HasNextLevel(_data);
        RefreshCosts(manager, hasNext);

        if (nextValueArrow != null) nextValueArrow.SetActive(hasNext);
        nextValueText.gameObject.SetActive(hasNext);
        upgradeButton.gameObject.SetActive(hasNext);

        if (!hasNext) return;

        nextValueText.text = manager.GetNextLevel(_data).statValue.ToString("0.#");
        nextValueText.color = Color.green;
        upgradeButton.interactable = manager.CanAffordNextLevel(_data);

        if (upgradeButtonLabel != null)
            upgradeButtonLabel.text = "Upgrade";
    }

    private void RefreshCosts(ShipUpgradeManager manager, bool hasNext)
    {
        foreach (var slot in _costSlots)
            Destroy(slot.gameObject);
        _costSlots.Clear();

        if (!hasNext) return;

        foreach (var cost in manager.GetNextLevel(_data).costs)
        {
            var slot = Instantiate(costSlotPrefab, costContainer);
            slot.Setup(cost, manager.GetAvailableAmount(cost.material));
            _costSlots.Add(slot);
        }
    }

    public void OnUpgradeButtonPressed()
    {
        if (_data == null) return;
        ShipUpgradeManager.Instance.TryUpgrade(_data);
    }
}
