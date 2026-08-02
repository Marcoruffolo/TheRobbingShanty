using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class CoreLinkingTable : MonoBehaviour, IInteractable
{
    [SerializeField] private NavigationRunState navigationRunState;
    [SerializeField] private InventoryItemData navigationCore;
    [SerializeField] private GameObject[] coreSlots;

    [Header("Prompts")]
    [SerializeField] private string placeCorePrompt = "Colocar nucleo de navegacion";
    [SerializeField] private string noCorePrompt = "No tenes nucleos inestables";
    [SerializeField] private string completePrompt = "La puerta ya esta abierta";

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    private NavigationRunState State => navigationRunState != null ? navigationRunState : NavigationRunState.Instance;

    private int _lastKnownZoneIndex = int.MinValue;

    public string InteractionPrompt
    {
        get
        {
            NavigationRunState state = State;
            if (state == null || navigationCore == null) return string.Empty;

            int zoneIndex = state.CurrentZoneIndex;
            if (!state.IsZoneReady(zoneIndex)) return string.Empty;

            int placed = state.GetPlacedCores(zoneIndex);
            int required = state.GetRequiredCores(zoneIndex);
            if (state.HasRequiredPlacedCores(zoneIndex)) return completePrompt;

            PlayerInventoryHolder inventory = PlayerInventoryHolder.Instance;
            if (inventory == null || inventory.GetItemCount(navigationCore) <= 0)
                return $"{noCorePrompt} ({placed}/{required})";

            return $"{placeCorePrompt} ({placed}/{required})";
        }
    }

    private void OnEnable()
    {
        State.ZoneProgressChanged += HandleZoneProgressChanged;
        UpdateSlots();
    }

    private void OnDisable()
    {
        NavigationRunState state = State;
        if (state != null)
            state.ZoneProgressChanged -= HandleZoneProgressChanged;
    }

    private void Update()
    {
        NavigationRunState state = State;
        if (state == null || state.CurrentZoneIndex == _lastKnownZoneIndex) return;

        _lastKnownZoneIndex = state.CurrentZoneIndex;
        UpdateSlots();
    }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;

        NavigationRunState state = State;
        if (state == null || navigationCore == null)
            return;

        int zoneIndex = state.CurrentZoneIndex;
        if (!state.CanPlaceCore(zoneIndex))
            return;

        PlayerInventoryHolder inventory = interactor != null
            ? interactor.GetComponentInParent<PlayerInventoryHolder>()
            : PlayerInventoryHolder.Instance;

        if (inventory == null || inventory.GetItemCount(navigationCore) <= 0)
            return;

        if (!inventory.TryRemoveItem(navigationCore, 1))
            return;

        if (!state.PlaceCore(zoneIndex))
        {
            inventory.AddToInventory(navigationCore, 1);
            return;
        }

        UpdateSlots();
        interactSuccessful = true;
        OnInteractionComplete?.Invoke(this);

        if (interactor != null)
            StartCoroutine(EndInteractionNextFrame(interactor));
    }

    public void Interact() => Interact(null, out _);

    public void EndInteraction()
    {
    }

    private IEnumerator EndInteractionNextFrame(Interactor interactor)
    {
        yield return null;
        interactor.RequestEndInteraction();
    }

    private void HandleZoneProgressChanged(int changedZoneIndex, int placedCores, int requiredCores)
    {
        NavigationRunState state = State;
        if (state != null && changedZoneIndex == state.CurrentZoneIndex)
            UpdateSlots();
    }

    private void UpdateSlots()
    {
        if (coreSlots == null) return;

        NavigationRunState state = State;
        int zoneIndex = state != null ? state.CurrentZoneIndex : -1;
        int placed = state != null ? state.GetPlacedCores(zoneIndex) : 0;
        int required = state != null && state.IsZoneReady(zoneIndex) ? state.GetRequiredCores(zoneIndex) : 0;

        for (int i = 0; i < coreSlots.Length; i++)
        {
            if (coreSlots[i] == null) continue;
            coreSlots[i].SetActive(i < placed && i < required);
        }
    }
}