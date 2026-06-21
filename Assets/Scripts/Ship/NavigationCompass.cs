using UnityEngine;
using UnityEngine.Events;

public class NavigationCompass : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform needle;
    [SerializeField] private GameObject[] filledSlots;
    [SerializeField] private InventoryItemData navigationCore;
    [SerializeField] private SOVariableInt installedCores;
    [SerializeField] private float maximumSpeed = 720f;
    [SerializeField] private float speedReductionPerCore = 120f;
    [SerializeField] private float directionChangeInterval = 0.4f;

    private float _currentSpeed;
    private float _nextDirectionChange;

    public string InteractionPrompt
    {
        get
        {
            if (installedCores == null || navigationCore == null) return string.Empty;
            if (installedCores.Value >= filledSlots.Length) return "Brújula calibrada";

            PlayerInventoryHolder inventory = PlayerInventoryHolder.Instance;

            return inventory != null && inventory.GetItemCount(navigationCore) > 0
                ? "Insertar núcleo de navegación"
                : "Necesitás un núcleo de navegación";
        }
    }

    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    private void OnEnable()
    {
        if (installedCores != null)
            installedCores.OnValueChanged += UpdateSlots;

        UpdateSlots(installedCores != null ? installedCores.Value : 0);
        ChooseSpeed();
    }

    private void OnDisable()
    {
        if (installedCores != null)
            installedCores.OnValueChanged -= UpdateSlots;
    }

    private void Update()
    {
        if (needle == null || installedCores == null || filledSlots.Length == 0) return;

        if (Time.time >= _nextDirectionChange)
            ChooseSpeed();

        needle.Rotate(Vector3.up, _currentSpeed * Time.deltaTime, Space.Self);
    }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        interactSuccessful = false;

        if (installedCores == null || navigationCore == null || installedCores.Value >= filledSlots.Length) return;

        PlayerInventoryHolder inventory = interactor != null
            ? interactor.GetComponentInParent<PlayerInventoryHolder>()
            : PlayerInventoryHolder.Instance;

        if (inventory == null || !inventory.TryRemoveItem(navigationCore, 1)) return;

        installedCores.Add(1);
        ChooseSpeed();
    }

    public void Interact()
    {
    }

    public void EndInteraction()
    {
    }

    private void UpdateSlots(int installedAmount)
    {
        for (int i = 0; i < filledSlots.Length; i++)
            if (filledSlots[i] != null)
                filledSlots[i].SetActive(i < installedAmount);
    }

    private void ChooseSpeed()
    {
        int amount = installedCores != null ? installedCores.Value : 0;
        float availableSpeed = Mathf.Max(0f, maximumSpeed - speedReductionPerCore * amount);

        if (filledSlots.Length > 0 && amount >= filledSlots.Length)
            availableSpeed = 0f;

        float direction = Random.value < 0.5f ? -1f : 1f;
        _currentSpeed = availableSpeed * Random.Range(0.65f, 1f) * direction;
        _nextDirectionChange = Time.time + directionChangeInterval;
    }
}
