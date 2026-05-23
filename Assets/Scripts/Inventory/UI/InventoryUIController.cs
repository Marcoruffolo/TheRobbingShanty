using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour, IBidirectionalDepositHandler
{
    public DynamicInventoryDisplay chestPanel;
    public DynamicInventoryDisplay playerBackpackPanel;
    [SerializeField] private PlayerInventoryHolder playerInventory;

    
    private void Awake()
    {
        chestPanel.gameObject.SetActive(false);
        playerBackpackPanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        if (playerInventory == null)
            playerInventory = PlayerInventoryHolder.Instance;
    }

    private void OnEnable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
        PlayerInventoryHolder.OnPlayerBackpackDisplayRequested += DisplayPlayerBackpack;
        InventoryHolder.OnDynamicInventoryCloseRequested += CloseInventory;
        PlayerInventoryHolder.OnPlayerBackpackCloseRequested += ClosePlayerBackpack;
        ShipRepairUI.OnRepairCompleted += CloseAllPanels;
    }

    private void OnDisable()
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
        PlayerInventoryHolder.OnPlayerBackpackDisplayRequested -= DisplayPlayerBackpack;
        InventoryHolder.OnDynamicInventoryCloseRequested -= CloseInventory;
        PlayerInventoryHolder.OnPlayerBackpackCloseRequested -= ClosePlayerBackpack;
        ShipRepairUI.OnRepairCompleted -= CloseAllPanels;
    }

    private void DisplayInventory(InventorySystem invToDisplay)
    {
        PlayerCamera.LockCursor(false);
        chestPanel.gameObject.SetActive(true);
        chestPanel.RefreshDynamicInventory(invToDisplay);
        RepairDepositRouter.Register(this);
    }

    private void DisplayPlayerBackpack(InventorySystem invToDisplay) 
    {
        PlayerCamera.LockCursor(false);
        playerBackpackPanel.gameObject.SetActive(true);
        playerBackpackPanel.RefreshDynamicInventory(invToDisplay);
    }

    private void CloseInventory()
    {
        RepairDepositRouter.Unregister();
        chestPanel.gameObject.SetActive(false);
        if (!playerBackpackPanel.gameObject.activeSelf)
            PlayerCamera.LockCursor(true);
    }

    public bool TryDeposit(InventorySlot sourceSlot) => TryDeposit(sourceSlot, null);

    public bool TryDeposit(InventorySlot sourceSlot, InventorySystem sourceSystem)
    {
        if (sourceSlot?.ItemData == null) return false;

        var chestInv = chestPanel.InventorySystem;
        if (chestInv == null) return false;

        if (sourceSystem == chestInv)
        {
            if (playerInventory == null) return false;
            var target = playerInventory.IsBackpackOpen
                ? playerInventory.SecondaryInventorySystem
                : playerInventory.PrimaryInventorySystem;
            if (!target.AddToInventory(sourceSlot.ItemData, sourceSlot.StackSize)) return false;
        }
        else
        {
            if (!chestInv.AddToInventory(sourceSlot.ItemData, sourceSlot.StackSize)) return false;
        }

        sourceSlot.ClearSlot();
        return true;
    }

    private void ClosePlayerBackpack()
    {
        playerBackpackPanel.gameObject.SetActive(false);
        if (!chestPanel.gameObject.activeSelf && !RepairDepositRouter.HasActiveHandler)
            PlayerCamera.LockCursor(true);
    }

    private void CloseAllPanels()
    {
        chestPanel.gameObject.SetActive(false);
        playerBackpackPanel.gameObject.SetActive(false);
    }

        
}
