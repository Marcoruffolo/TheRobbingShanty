using System.Collections.Generic;
using UnityEngine;

public class ShipUpgradeUI : MonoBehaviour, IGameEventListener<bool>
{
    [Header("Events")]
    [SerializeField] private BoolGameEvent onShipUpgradeUIOpen;
    [SerializeField] private BoolGameEvent onItemUpgradeUIOpen;

    [Header("Data")]
    [SerializeField] private ShipUpgradeRegistry registry;

    [Header("Panel")]
    [SerializeField] private GameObject panel;

    [Header("Cards")]
    [SerializeField] private ShipUpgradeCardUI cardPrefab;
    [SerializeField] private Transform cardContainer;

    private readonly List<ShipUpgradeCardUI> _cards = new();
    private PlayerInventoryHolder _playerInventory;

    private void Awake() => panel.SetActive(false);

    private void Start()
    {
        _playerInventory = PlayerInventoryHolder.Instance;
    }

    private void OnEnable() => onShipUpgradeUIOpen.RegisterListener(this);

    private void OnDisable() => onShipUpgradeUIOpen.UnregisterListener(this);

    public void OnEventRaised(bool isOpen)
    {
        if (isOpen) Open();
        else Close();
    }

    private void Open()
    {
        onItemUpgradeUIOpen?.Raise(false);
        panel.SetActive(true);
        PopulateCards(registry != null ? registry.upgrades : null);
    }

    private void Close()
    {
        panel.SetActive(false);
        ClearCards();
    }

    public void RequestClose() => onShipUpgradeUIOpen.Raise(false);

    private void PopulateCards(List<ShipUpgradeData> upgrades)
    {
        ClearCards();
        if (upgrades == null) return;

        foreach (var upgrade in upgrades)
        {
            var card = Instantiate(cardPrefab, cardContainer);
            card.Setup(upgrade, _playerInventory);
            _cards.Add(card);
        }
    }

    private void ClearCards()
    {
        foreach (var card in _cards)
            Destroy(card.gameObject);
        _cards.Clear();
    }
}
