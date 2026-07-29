using UnityEngine;

[CreateAssetMenu(fileName = "RangedWeapon", menuName = "Inventory/Ranged Weapon Item")]
public class RangedWeaponItemData : EquippableWeaponData
{
    public Bullet bulletPrefab;
    public int poolSize = 1;
    public float baseDamage = 10f;
    public float bulletSpeed = 40f;
    public float maxDistance = 50f;
    public float reloadTime = 1f;
    public AudioClip reloadSound;
    public ItemUpgradeData damageUpgrade;

    public override ItemUpgradeData DamageUpgrade => damageUpgrade;
    public override float BaseDamage => baseDamage;
}
