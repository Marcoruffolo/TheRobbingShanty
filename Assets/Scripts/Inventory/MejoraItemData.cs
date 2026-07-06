using UnityEngine;

[CreateAssetMenu(fileName = "MejoraItemData", menuName = "Inventory/MejoraItemData")]
public class MejoraItemData : InventoryItemData
{
    [SerializeField] private IntGameEvent onPlayerEffected;
    public int EffectAmount;
    public override void UseItem()
    {
        onPlayerEffected.Raise(EffectAmount);
    }
}
