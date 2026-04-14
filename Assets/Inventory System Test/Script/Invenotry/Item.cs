using UnityEngine;

public class Item : MonoBehaviour
{
    public InventoryItemData ItemData;

    public void PickUpItem(PlayerInventoryHolder playerInventory)
    {
        if(!playerInventory) return;

        if(playerInventory.AddToInventory(ItemData, 1))
        {
            Destroy(gameObject);
        }
    }
}
