using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Inventory/Item")]
public class InventoryItemData : ScriptableObject
{
    public int ID;
    public string DisplayName;
    [TextArea(2, 5)]
    public string Description;
 
    public Sprite Icon;
    public int maxStackSize = 64;
    public bool CanDrop = true;

    public GameObject itemPrefab;
    public GameObject handItemPrefab;

    public ItemType itemtype = ItemType.None;

    public virtual void UseItem()
    {
        //la arma ya tiene su script
        if (itemtype == ItemType.Consumible)
        {
            Debug.Log("Consumido");
        }
        else if(itemtype == ItemType.Mejora)
        {
            Debug.Log("Mejorado");
        }
    }

}

public enum ItemType
{
    None,
    Consumible,
    Arma,
    Mejora
}
