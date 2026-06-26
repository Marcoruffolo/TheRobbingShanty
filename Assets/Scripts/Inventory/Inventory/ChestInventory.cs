using System;
using UnityEngine;
using UnityEngine.Events;

public class ChestInventory : InventoryHolder, IInteractable
{
    [SerializeField] private ChestLootTable lootTable;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public string InteractionPrompt => string.Empty;

    public event Action<bool> OpenChestInventory;

    protected override void Awake()
    {
        base.Awake();
        if (lootTable != null) GenerateLoot();
    }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        OnDynamicInventoryDisplayRequested?.Invoke(primaryInventorySystem);
        interactSuccessful = true;
        BlockPlayerMovement.Instance?.ImmobilizePlayer();
        OpenChestInventory?.Invoke(true);
    }

    public void Interact()
    {
        Interact(null, out _);
    }

    public void EndInteraction()
    {
        OnDynamicInventoryCloseRequested?.Invoke();
        OnInteractionComplete?.Invoke(this);
        BlockPlayerMovement.Instance?.FreePlayer();
        OpenChestInventory?.Invoke(false);
    }

    private void GenerateLoot() => LootGenerator.Generate(lootTable, primaryInventorySystem);
}
