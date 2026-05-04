using UnityEngine;

public class ShipDurabilityUI : MonoBehaviour
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
    }

    private void Close()
    {
        slot.OnWoodDeposited -= HandleWoodDeposited;
        panel.SetActive(false);
    }

    private void HandleWoodDeposited() => durability.Add(durabilityPerWood);
}
