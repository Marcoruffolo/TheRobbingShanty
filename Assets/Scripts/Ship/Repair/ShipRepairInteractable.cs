using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ShipRepairInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private ShipRepairData repairData;
    [SerializeField] private SOVariableInt shipRepairLevel;
    [SerializeField] private SceneField mainScene;

    [Header("Prompts")]
    [SerializeField] private string repairPrompt = "Repair Ship";
    [SerializeField] private string helmPrompt = "Take Wheel";

    public string InteractionPrompt => IsRepaired ? helmPrompt : repairPrompt;
    public UnityAction<IInteractable> OnInteractionComplete { get; set; }

    public static UnityAction<ShipRepairData> OnRepairUIOpened;
    public static UnityAction OnRepairUIClosed;

    private static readonly HashSet<string> _repairedShips = new HashSet<string>();

    private Interactor _interactor;
    private bool IsRepaired => repairData != null && _repairedShips.Contains(repairData.name);

    private void OnEnable()
    {
        ShipRepairUI.OnCloseRequested += HandleCloseRequested;
        ShipRepairUI.OnRepairCompleted += HandleRepairCompleted;
        ShipRepairUI.OnReturnWithoutRepair += HandleReturnWithoutRepair;
    }

    private void OnDisable()
    {
        ShipRepairUI.OnCloseRequested -= HandleCloseRequested;
        ShipRepairUI.OnRepairCompleted -= HandleRepairCompleted;
        ShipRepairUI.OnReturnWithoutRepair -= HandleReturnWithoutRepair;
    }

    private void HandleCloseRequested() => _interactor?.RequestEndInteraction();
    private void HandleReturnWithoutRepair() => LoadMainScene();
    private void HandleRepairCompleted()
    {
        _repairedShips.Add(repairData.name);
        shipRepairLevel?.Add(1);
    }

    private void LoadMainScene()
    {
        if (SceneLoader.Instance != null)
            SceneLoader.Instance.TryLoadScene(mainScene);
        else
            SceneManager.LoadScene(mainScene.SceneName);
    }

    public void Interact(Interactor interactor, out bool interactSuccessful)
    {
        if (IsRepaired)
        {
            LoadMainScene();
            interactSuccessful = true;
            return;
        }

        _interactor = interactor;
        PlayerCamera.LockCursor(false);
        BlockPlayerMovement.Instance?.ImmobilizePlayer();
        OnRepairUIOpened?.Invoke(repairData);
        interactSuccessful = true;
    }

    public void Interact() => Interact(null, out _);

    public void EndInteraction()
    {
        OnRepairUIClosed?.Invoke();
        PlayerCamera.LockCursor(true);
        BlockPlayerMovement.Instance?.FreePlayer();
        OnInteractionComplete?.Invoke(this);
    }
}
