using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInventoryHolder : InventoryHolder
{
    [SerializeField] protected int secondaryInventorySize;
    [SerializeField] protected InventorySystem secondaryInventorySystem;
    [SerializeField] private SOInventorySaveData inventorySaveData;
    public bool IsBackpackOpen { get; private set; }
    public InventorySystem SecondaryInventorySystem => secondaryInventorySystem;

    public static PlayerInventoryHolder Instance { get; private set; }

    public static UnityAction<InventorySystem> OnPlayerBackpackDisplayRequested;
    public static UnityAction OnPlayerBackpackCloseRequested;
    public static UnityAction<InventoryItemData> OnHotbarSelectionChanged;
    private int selectedHotbarIndex = 0;
    private GameObject currentHandItemInstance;
    private InventoryItemData lastHandItemData;

    [Header("Hand Item")]
    [SerializeField] private Transform handItemSpawnPoint;

    [Header("Item Drop")]
    [SerializeField] private Transform dropPoint;

    protected override void Awake()
    {
        Instance = this;
        base.Awake();
        secondaryInventorySystem = new InventorySystem(secondaryInventorySize);
        inventorySaveData?.Restore(primaryInventorySystem, secondaryInventorySystem);
        primaryInventorySystem.OnInventorySlotChanged += OnPrimarySlotChanged;
    }

    private void OnDestroy()
    {
        primaryInventorySystem.OnInventorySlotChanged -= OnPrimarySlotChanged;
        inventorySaveData?.Save(primaryInventorySystem, secondaryInventorySystem);
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
        if (Keyboard.current.tabKey.wasPressedThisFrame && !IsBackpackOpen)
        {
            OnPlayerBackpackDisplayRequested?.Invoke(secondaryInventorySystem);
            IsBackpackOpen = true;
        }
        else if (Keyboard.current.tabKey.wasPressedThisFrame && IsBackpackOpen)
        {
            OnPlayerBackpackCloseRequested?.Invoke();
            IsBackpackOpen = false;
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
        if (currentHandItemInstance != null)
        {
            Destroy(currentHandItemInstance);
            currentHandItemInstance = null;
        }

        var slot = primaryInventorySystem.InventorySlots[selectedHotbarIndex];
        lastHandItemData = slot.ItemData;

        bool isWeapon = slot.ItemData is WeaponItemData;
        if (!isWeapon && slot.ItemData != null && slot.ItemData.handItemPrefab != null && handItemSpawnPoint != null)
            currentHandItemInstance = Instantiate(slot.ItemData.handItemPrefab, handItemSpawnPoint.position, handItemSpawnPoint.rotation, handItemSpawnPoint);

        OnHotbarSelectionChanged?.Invoke(slot.ItemData);
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

    public void ClearInventory()
    {
        foreach (var slot in primaryInventorySystem.InventorySlots)
            slot.ClearSlot();

        foreach (var slot in secondaryInventorySystem.InventorySlots)
            slot.ClearSlot();
    }



    public int SelectedHotbarIndex => selectedHotbarIndex;
}