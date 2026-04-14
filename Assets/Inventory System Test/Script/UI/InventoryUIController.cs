using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUIController : MonoBehaviour
{
    public DynamicInventoryDisplay inventoryPanel;
    
    private void Awake() 
    {
        inventoryPanel.gameObject.SetActive(false);    
    }

    private void OnEnable() 
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested += DisplayInventory;
        InventoryHolder.OnDynamicInventoryCloseRequested += CloseInventory;
    }

    private void OnDisable() 
    {
        InventoryHolder.OnDynamicInventoryDisplayRequested -= DisplayInventory;
        InventoryHolder.OnDynamicInventoryCloseRequested -= CloseInventory;
    }

    private void DisplayInventory(InventorySystem invToDisplay) 
    {
        inventoryPanel.gameObject.SetActive(true);
        inventoryPanel.RefreshDynamicInventory(invToDisplay);
    }

    private void CloseInventory() 
    {
        inventoryPanel.gameObject.SetActive(false);
    }

        
}
