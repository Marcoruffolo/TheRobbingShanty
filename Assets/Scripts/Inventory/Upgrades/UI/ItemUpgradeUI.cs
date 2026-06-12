using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class ItemUpgradeUI : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject detailPanel;

    [Header("List")]
    [SerializeField] private ItemUpgradeEntryUI entryPrefab;
    [SerializeField] private Transform entryContainer;

    [Header("Detail")]
    [SerializeField] private Image detailIcon;
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text statNameText;
    [SerializeField] private TMP_Text currentValueText;
    [SerializeField] private TMP_Text nextValueText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private TMP_Text upgradeButtonLabel;

    [Header("Costs")]
    [SerializeField] private UpgradeCostSlotUI costSlotPrefab;
    [SerializeField] private Transform costContainer;

    public static UnityAction OnCloseRequested;

    private readonly List<ItemUpgradeEntryUI> _entries = new();
    private readonly List<UpgradeCostSlotUI> _costSlots = new();
    private ItemUpgradeEntryUI _selectedEntry;
    private ItemUpgradeData _selectedUpgrade;
    private PlayerInventoryHolder _playerInventory;

    private void Awake()
    {
        panel.SetActive(false);
        detailPanel.SetActive(false);
    }

    private void Start()
    {
        _playerInventory = PlayerInventoryHolder.Instance;
    }

    private void OnEnable()
    {
        ItemUpgradeStation.OnUpgradeUIOpened += Open;
        ItemUpgradeStation.OnUpgradeUIClosed += Close;
        ItemUpgradeManager.OnUpgradeApplied += HandleUpgradeApplied;
    }

    private void OnDisable()
    {
        ItemUpgradeStation.OnUpgradeUIOpened -= Open;
        ItemUpgradeStation.OnUpgradeUIClosed -= Close;
        ItemUpgradeManager.OnUpgradeApplied -= HandleUpgradeApplied;
        UnsubscribeInventoryEvents();
    }

    private void Open(List<ItemUpgradeData> upgrades)
    {
        panel.SetActive(true);
        detailPanel.SetActive(false);
        _selectedEntry = null;
        _selectedUpgrade = null;

        SubscribeInventoryEvents();
        PopulateEntries(upgrades);
    }

    private void Close()
    {
        UnsubscribeInventoryEvents();
        panel.SetActive(false);
        _selectedEntry = null;
        _selectedUpgrade = null;
    }

    public void RequestClose() => OnCloseRequested?.Invoke();

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

    private void HandleInventoryChanged(InventorySlot _) => RefreshDetail();

    private void HandleUpgradeApplied(ItemUpgradeData data)
    {
        if (data == _selectedUpgrade) RefreshDetail();
    }

    private void PopulateEntries(List<ItemUpgradeData> upgrades)
    {
        foreach (var entry in _entries)
            Destroy(entry.gameObject);
        _entries.Clear();

        if (upgrades == null) return;

        foreach (var upgrade in upgrades)
        {
            var entry = Instantiate(entryPrefab, entryContainer);
            entry.Setup(upgrade, this);
            _entries.Add(entry);
        }
    }

    public void SelectUpgrade(ItemUpgradeEntryUI entry)
    {
        _selectedEntry?.SetSelected(false);
        _selectedEntry = entry;
        _selectedEntry.SetSelected(true);

        _selectedUpgrade = entry.UpgradeData;
        detailPanel.SetActive(true);
        RefreshDetail();
    }

    private void RefreshDetail()
    {
        if (_selectedUpgrade == null) return;

        var manager = ItemUpgradeManager.Instance;
        var item = _selectedUpgrade.targetItem;

        if (detailIcon != null && item?.Icon != null)
            detailIcon.sprite = item.Icon;

        if (itemNameText != null)
            itemNameText.text = item != null ? item.DisplayName : "";

        if (statNameText != null)
            statNameText.text = _selectedUpgrade.statName;

        currentValueText.text = manager.GetCurrentValue(_selectedUpgrade).ToString("0.#");

        bool hasNext = manager.HasNextLevel(_selectedUpgrade);
        RefreshCosts(manager, hasNext);

        nextValueText.gameObject.SetActive(hasNext);
        upgradeButton.gameObject.SetActive(hasNext);

        if (!hasNext) return;

        nextValueText.text = manager.GetNextLevel(_selectedUpgrade).statValue.ToString("0.#");
        upgradeButton.interactable = manager.CanAffordNextLevel(_selectedUpgrade);

        if (upgradeButtonLabel != null)
            upgradeButtonLabel.text = "Upgrade";
    }

    private void RefreshCosts(ItemUpgradeManager manager, bool hasNext)
    {
        foreach (var slot in _costSlots)
            Destroy(slot.gameObject);
        _costSlots.Clear();

        if (!hasNext) return;

        foreach (var cost in manager.GetNextLevel(_selectedUpgrade).costs)
        {
            var slot = Instantiate(costSlotPrefab, costContainer);
            slot.Setup(cost, manager.GetAvailableAmount(cost.material));
            _costSlots.Add(slot);
        }
    }

    public void OnUpgradeButtonPressed()
    {
        if (_selectedUpgrade == null) return;

        ItemUpgradeManager.Instance.TryUpgrade(_selectedUpgrade);
    }
}
