using UnityEngine;

public class ShipDurabilityUI : MonoBehaviour, IItemDepositHandler
{
    [SerializeField] private GameObject panel;
    [SerializeField] private ShipDurabilitySlotUI slot;
    [SerializeField] private SOVariableFloat durability;
    [SerializeField] private InventoryItemData woodItemData;
    [SerializeField] private MouseItemData mouseItemData;
    [SerializeField] private float durabilityPerWood = 10f;

    private void Awake() => panel.SetActive(false);

    private void OnEnable()
    {
        ShipDurabilityInteractable.OnDurabilityUIOpened += Open;
        ShipDurabilityInteractable.OnDurabilityUIClosed += Close;
    }

    private void OnDisable()
    {
        ShipDurabilityInteractable.OnDurabilityUIOpened -= Open;
        ShipDurabilityInteractable.OnDurabilityUIClosed -= Close;
    }

    private void Open()
    {
        panel.SetActive(true);
        slot.Setup(woodItemData, mouseItemData, durability);
        slot.OnWoodDeposited += HandleWoodDeposited;
        RepairDepositRouter.Register(this);
    }

    private void Close()
    {
        slot.OnWoodDeposited -= HandleWoodDeposited;
        RepairDepositRouter.Unregister();
        panel.SetActive(false);
    }

    private void HandleWoodDeposited() => durability.Add(durabilityPerWood);

    public bool TryDeposit(InventorySlot sourceSlot)
    {
        if (sourceSlot?.ItemData != woodItemData) return false;

        int deposited = 0;
        while (sourceSlot.StackSize > 0)
        {
            if (durability.IsClamped && durability.Value >= durability.Max) break;
            sourceSlot.RemoveFromStack(1);
            HandleWoodDeposited();
            deposited++;
        }

        if (sourceSlot.StackSize <= 0) sourceSlot.ClearSlot();
        return deposited > 0;
    }
}
