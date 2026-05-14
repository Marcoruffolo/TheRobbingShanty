using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    public DynamicInventoryDisplay chestPanel;
    public DynamicInventoryDisplay playerBackpackPanel;

    
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
    }

    private void DisplayPlayerBackpack(InventorySystem invToDisplay) 
    {
        PlayerCamera.LockCursor(false);
        playerBackpackPanel.gameObject.SetActive(true);
        playerBackpackPanel.RefreshDynamicInventory(invToDisplay);
    }

    private void CloseInventory() 
    {
        PlayerCamera.LockCursor(true);
        chestPanel.gameObject.SetActive(false);
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
