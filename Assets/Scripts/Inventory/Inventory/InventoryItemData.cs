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
 

}

public enum ItemType
{
    None,
    Consumible,
    Arma,
    Mejora
}
