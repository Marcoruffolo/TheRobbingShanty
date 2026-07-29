public abstract class EquippableWeaponData : InventoryItemData
{
    public abstract ItemUpgradeData DamageUpgrade { get; }
    public abstract float BaseDamage { get; }

    public float GetCurrentDamage()
    {
        if (DamageUpgrade != null && ItemUpgradeManager.Instance != null)
            return ItemUpgradeManager.Instance.GetCurrentValue(DamageUpgrade);

        return BaseDamage;
    }
}
