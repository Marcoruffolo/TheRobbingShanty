using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InventoryHolder : MonoBehaviour
{
    [SerializeField] private int inventorySize;
    [SerializeField] protected InventorySystem primaryInventorySystem;

    public InventorySystem PrimaryInventorySystem => primaryInventorySystem;

    public static UnityAction<InventorySystem> OnDynamicInventoryDisplayRequested;
    
    public static UnityAction OnDynamicInventoryCloseRequested;

    protected virtual void Awake() {
        primaryInventorySystem = new InventorySystem(inventorySize);
    }
}
