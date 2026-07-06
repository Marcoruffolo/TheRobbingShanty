using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(EnemyBase))]
public class EnemyInventory : InventoryHolder, IInteractable
{
    [Header("Loot")]
    [SerializeField] private ChestLootTable lootTable;
    [SerializeField] private Collider lootTrigger;

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }
    public string InteractionPrompt => string.Empty;

    private bool _canInteract;
    private bool _isOpen;
    private Interactor _currentInteractor;
    private EnemyBase _enemyBase;
    private EnemyRagdoll _ragdoll;

    protected override void Awake()
    {
        base.Awake();
        _enemyBase = GetComponent<EnemyBase>();
        _ragdoll = GetComponent<EnemyRagdoll>();
        if (lootTrigger != null) lootTrigger.enabled = false;
    }

    private void OnEnable()  => _enemyBase.OnDeath += OnEnemyDied;
    private void OnDisable() => _enemyBase.OnDeath -= OnEnemyDied;

    private void OnEnemyDied()
    {
        if (lootTrigger != null && _ragdoll != null)
            lootTrigger.transform.SetParent(_ragdoll.RagdollAnchor, true);

        if (lootTable == null) return;

        if (primaryInventorySystem.InventorySize < lootTable.entries.Count)
            primaryInventorySystem = new InventorySystem(lootTable.entries.Count);

        LootGenerator.Generate(lootTable, primaryInventorySystem);
        _canInteract = !primaryInventorySystem.IsEmpty();
        if (lootTrigger != null) lootTrigger.enabled = _canInteract;
    }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        if (!_canInteract) { interactSuccessful = false; return; }

        _currentInteractor = interactor;
        _isOpen = true;
        primaryInventorySystem.OnInventorySlotChanged += OnSlotChanged;
        OnDynamicInventoryDisplayRequested?.Invoke(primaryInventorySystem);
        BlockPlayerMovement.Instance?.ImmobilizePlayer();
        interactSuccessful = true;
    }

    public void Interact() => Interact(null, out _);

    public void EndInteraction()
    {
        if (!_isOpen) return;
        _isOpen = false;
        primaryInventorySystem.OnInventorySlotChanged -= OnSlotChanged;
        OnDynamicInventoryCloseRequested?.Invoke();
        OnInteractionComplete?.Invoke(this);
        BlockPlayerMovement.Instance?.FreePlayer();
        _currentInteractor = null;
    }

    private void OnSlotChanged(InventorySlot _)
    {
        if (!primaryInventorySystem.IsEmpty()) return;
        _canInteract = false;
        if (lootTrigger != null) lootTrigger.enabled = false;
        if (_currentInteractor != null)
            _currentInteractor.RequestEndInteraction();
        else
            EndInteraction();
    }
}
