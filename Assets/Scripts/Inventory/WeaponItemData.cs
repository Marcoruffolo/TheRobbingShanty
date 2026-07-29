using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Inventory/Weapon Item")]
public class WeaponItemData : EquippableWeaponData
{
    public WeaponStats stats = new WeaponStats();
    public ItemUpgradeData damageUpgrade;

    public override ItemUpgradeData DamageUpgrade => damageUpgrade;
    public override float BaseDamage => stats.attackDamage;
}
