using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ItemUpgradeManager : MonoBehaviour
{
    public static ItemUpgradeManager Instance { get; private set; }

    [SerializeField] private PlayerInventoryHolder playerInventory;

    private readonly Dictionary<ItemUpgradeData, int> _levels = new();

    public static UnityAction<ItemUpgradeData> OnUpgradeApplied;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (playerInventory == null)
            playerInventory = PlayerInventoryHolder.Instance;
    }

    public int GetLevel(ItemUpgradeData data) =>
        _levels.TryGetValue(data, out int level) ? level : 0;

    public float GetCurrentValue(ItemUpgradeData data)
    {
        int level = GetLevel(data);
        return level == 0 ? data.baseValue : data.levels[level - 1].statValue;
    }

    public bool HasNextLevel(ItemUpgradeData data) => GetLevel(data) < data.levels.Count;

    public ItemUpgradeLevel GetNextLevel(ItemUpgradeData data) =>
        HasNextLevel(data) ? data.levels[GetLevel(data)] : null;

    public int GetAvailableAmount(InventoryItemData item)
    {
        if (playerInventory == null) return 0;

        return playerInventory.PrimaryInventorySystem.GetItemCount(item)
             + playerInventory.SecondaryInventorySystem.GetItemCount(item);
    }

    public bool CanAffordNextLevel(ItemUpgradeData data)
    {
        var next = GetNextLevel(data);
        if (next == null) return false;

        foreach (var cost in next.costs)
        {
            if (cost.material == null) return false;
            if (GetAvailableAmount(cost.material) < cost.amount) return false;
        }

        return true;
    }

    public bool TryUpgrade(ItemUpgradeData data)
    {
        if (!CanAffordNextLevel(data)) return false;

        foreach (var cost in GetNextLevel(data).costs)
            ConsumeItem(cost.material, cost.amount);

        _levels[data] = GetLevel(data) + 1;
        OnUpgradeApplied?.Invoke(data);
        return true;
    }

    private void ConsumeItem(InventoryItemData item, int amount)
    {
        int fromPrimary = Mathf.Min(amount, playerInventory.PrimaryInventorySystem.GetItemCount(item));
        if (fromPrimary > 0) playerInventory.PrimaryInventorySystem.RemoveItem(item, fromPrimary);

        int remaining = amount - fromPrimary;
        if (remaining > 0) playerInventory.SecondaryInventorySystem.RemoveItem(item, remaining);
    }
}
