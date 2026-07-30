using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShipUpgradeManager : MonoBehaviour
{
    public static ShipUpgradeManager Instance { get; private set; }

    [SerializeField] private PlayerInventoryHolder playerInventory;
    [SerializeField] private SOVariableFloat durability;
    [SerializeField] private SOVariableFloat speed;

    private readonly Dictionary<ShipUpgradeData, int> _levels = new();

    public static UnityAction<ShipUpgradeData> OnUpgradeApplied;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (playerInventory == null)
            playerInventory = PlayerInventoryHolder.Instance;
    }

    public int GetLevel(ShipUpgradeData data) =>
        _levels.TryGetValue(data, out int level) ? level : 0;

    public float GetCurrentValue(ShipUpgradeData data)
    {
        int level = GetLevel(data);
        return level == 0 ? data.baseValue : data.levels[level - 1].statValue;
    }

    public bool HasNextLevel(ShipUpgradeData data) => GetLevel(data) < data.levels.Count;

    public ItemUpgradeLevel GetNextLevel(ShipUpgradeData data) =>
        HasNextLevel(data) ? data.levels[GetLevel(data)] : null;

    public int GetAvailableAmount(InventoryItemData item)
    {
        if (playerInventory == null) return 0;

        return playerInventory.PrimaryInventorySystem.GetItemCount(item)
             + playerInventory.SecondaryInventorySystem.GetItemCount(item);
    }

    public bool CanAffordNextLevel(ShipUpgradeData data)
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

    public bool TryUpgrade(ShipUpgradeData data)
    {
        if (!CanAffordNextLevel(data)) return false;

        float newValue = GetNextLevel(data).statValue;

        foreach (var cost in GetNextLevel(data).costs)
            ConsumeItem(cost.material, cost.amount);

        _levels[data] = GetLevel(data) + 1;
        ApplyStat(data.stat, newValue);

        OnUpgradeApplied?.Invoke(data);
        return true;
    }

    private void ApplyStat(ShipStat stat, float value)
    {
        switch (stat)
        {
            case ShipStat.MaxDurability:
                if (durability != null) durability.Max = value;
                break;
            case ShipStat.Speed:
                if (speed != null) speed.SetValue(value);
                break;
        }
    }

    private void ConsumeItem(InventoryItemData item, int amount)
    {
        int fromPrimary = Mathf.Min(amount, playerInventory.PrimaryInventorySystem.GetItemCount(item));
        if (fromPrimary > 0) playerInventory.PrimaryInventorySystem.RemoveItem(item, fromPrimary);

        int remaining = amount - fromPrimary;
        if (remaining > 0) playerInventory.SecondaryInventorySystem.RemoveItem(item, remaining);
    }
}
