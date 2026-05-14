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
        PlayerCamera.LockCursor(true);
        chestPanel.gameObject.SetActive(false);
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
            if (!playerInventory.AddToInventory(sourceSlot.ItemData, sourceSlot.StackSize)) return false;
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
        PlayerCamera.LockCursor(true);
        playerBackpackPanel.gameObject.SetActive(false);
    }

    private void CloseAllPanels()
    {
        chestPanel.gameObject.SetActive(false);
        playerBackpackPanel.gameObject.SetActive(false);
    }

        
}
