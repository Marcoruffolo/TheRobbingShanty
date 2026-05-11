using UnityEngine;
using UnityEngine.Events;

public class ChestInventory : InventoryHolder, IInteractable
{
    [Header("Loot")]
    [SerializeField] private ChestLootTable lootTable;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public string InteractionPrompt => string.Empty;

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
    }

    private void GenerateLoot()
    {
        foreach (var entry in lootTable.entries)
        {
            if (entry.item == null) continue;
            if (Random.value <= entry.probability)
            {
                int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                primaryInventorySystem.AddToInventory(entry.item, amount);
            }
        }
    }
}
