using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour
{
    public InventoryItemData ItemData;

    public event UnityAction PickedUp;

    public void PickUpItem(PlayerInventoryHolder playerInventory)
    {
        if(!playerInventory) return;

        if(playerInventory.AddToInventory(ItemData, 1))
        {
            PickedUp?.Invoke();
            Destroy(gameObject);
        }
    }
}
