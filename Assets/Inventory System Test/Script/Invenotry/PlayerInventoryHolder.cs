using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInventoryHolder : InventoryHolder
{
    [SerializeField] protected int secondaryInventorySize;
    [SerializeField] protected InventorySystem secondaryInventorySystem;
    bool inventoryOpen = false;
    public InventorySystem SecondaryInventorySystem => secondaryInventorySystem;

    private int selectedHotbarIndex = 0;
    private GameObject currentHandItemInstance;
    private InventoryItemData lastHandItemData;

    [Header("Hand Item")]
    [SerializeField] private Transform handItemSpawnPoint;

    [Header("Item Drop")]
    [SerializeField] private Transform dropPoint;

    protected override void Awake()
    {
        base.Awake();
        secondaryInventorySystem = new InventorySystem(secondaryInventorySize);

        primaryInventorySystem.OnInventorySlotChanged += OnPrimarySlotChanged;
    }

    private void OnDestroy()
    {
        primaryInventorySystem.OnInventorySlotChanged -= OnPrimarySlotChanged;
    }

    private void OnPrimarySlotChanged(InventorySlot changedSlot)
    {
        if (primaryInventorySystem.InventorySlots[selectedHotbarIndex] == changedSlot)
        {
            UpdateHandItem();
        }
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame && !inventoryOpen)
        {
            OnDynamicInventoryDisplayRequested?.Invoke(secondaryInventorySystem);
            inventoryOpen = true;
        }
        else if (Keyboard.current.tabKey.wasPressedThisFrame && inventoryOpen)
        {
            OnDynamicInventoryCloseRequested?.Invoke();
            inventoryOpen = false;
        }

        HandleHotbarInput();

        var currentSlotItem = primaryInventorySystem.InventorySlots[selectedHotbarIndex].ItemData;
        if (currentSlotItem != lastHandItemData)
        {
            UpdateHandItem();
        }

        if (Keyboard.current.qKey.wasPressedThisFrame)
        {
            DropHotbarItem();
        }
    }

    private void HandleHotbarInput()
    {
        int newIndex = -1;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) newIndex = 0;
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) newIndex = 1;
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) newIndex = 2;
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) newIndex = 3;
        else if (Keyboard.current.digit5Key.wasPressedThisFrame) newIndex = 4;
        else if (Keyboard.current.digit6Key.wasPressedThisFrame) newIndex = 5;
        else if (Keyboard.current.digit7Key.wasPressedThisFrame) newIndex = 6;
        else if (Keyboard.current.digit8Key.wasPressedThisFrame) newIndex = 7;
        else if (Keyboard.current.digit9Key.wasPressedThisFrame) newIndex = 8;
        else if (Keyboard.current.digit0Key.wasPressedThisFrame) newIndex = 9;

        if (newIndex >= 0 && newIndex < primaryInventorySystem.InventorySize)
        {
            selectedHotbarIndex = newIndex;
            UpdateHandItem();
        }
    }

    private void UpdateHandItem()
    {
        // Destroy previous hand item
        if (currentHandItemInstance != null)
        {
            Destroy(currentHandItemInstance);
            currentHandItemInstance = null;
        }

        var slot = primaryInventorySystem.InventorySlots[selectedHotbarIndex];
        lastHandItemData = slot.ItemData;

        if (slot.ItemData != null && slot.ItemData.handItemPrefab != null && handItemSpawnPoint != null)
        {
            currentHandItemInstance = Instantiate(slot.ItemData.handItemPrefab, handItemSpawnPoint.position, handItemSpawnPoint.rotation, handItemSpawnPoint);
        }
    }

    private void DropHotbarItem()
    {
        var slot = primaryInventorySystem.InventorySlots[selectedHotbarIndex];
        if (slot.ItemData == null) return;

        DropItem(slot.ItemData, slot.StackSize);

        slot.ClearSlot();
        primaryInventorySystem.OnInventorySlotChanged?.Invoke(slot);
        UpdateHandItem();
    }

    public void DropItem(InventoryItemData itemData, int amount = 1)
    {
        if (itemData == null || itemData.itemPrefab == null) return;

        Vector3 spawnPos = dropPoint != null ? dropPoint.position : transform.position + transform.forward * 1f;
        Quaternion spawnRot = dropPoint != null ? dropPoint.rotation : Quaternion.identity;

        for (int i = 0; i < amount; i++)
        {
            Vector3 offset = Random.insideUnitSphere * 0.2f;
            offset.y = 0f;
            Instantiate(itemData.itemPrefab, spawnPos + offset, spawnRot);
        }
    }

    public bool AddToInventory(InventoryItemData data, int amount)
    {
        if (primaryInventorySystem.AddToInventory(data, amount))
        {
            return true;
        }
        else if (secondaryInventorySystem.AddToInventory(data, amount))
        {
            return true;
        }

        return false;
    }

    public int SelectedHotbarIndex => selectedHotbarIndex;
}