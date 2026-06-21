using UnityEngine;

public class PlayerRangedCombat : MonoBehaviour
{
    [SerializeField] private Transform weaponSpawnPoint;
    [SerializeField] private SOVariableFloat reloadProgress;
    public AudioClip fireSound;

    private GameObject _currentWeaponModel;
    private RangedWeaponItemData _currentWeapon;
    private BulletPool _bulletPool;

    private AudioSource _audioSource;
    private PlayerInputHandler _inputHandler;
    private Camera _cam;

    private bool _isReloading;
    private float _reloadTimer;

    public bool IsAiming { get; private set; }

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _cam = Camera.main;
        if (reloadProgress != null) reloadProgress.Value = 1f;
    }

    void Start()
    {
        _inputHandler = PlayerInputHandler.Instance;
        if (_inputHandler == null)
        {
            Debug.LogError("[PlayerRangedCombat] PlayerInputHandler instance not found.");
            return;
        }
        _inputHandler.OnFire += Fire;
        PlayerInventoryHolder.OnHotbarSelectionChanged += OnSelectionChanged;
    }

    void OnDestroy()
    {
        if (_inputHandler != null) _inputHandler.OnFire -= Fire;
        PlayerInventoryHolder.OnHotbarSelectionChanged -= OnSelectionChanged;
    }

    void Update()
    {
        IsAiming = _currentWeapon != null && _inputHandler != null && _inputHandler.AimHeld;

        if (!_isReloading) return;

        _reloadTimer += Time.deltaTime;
        if (reloadProgress != null) reloadProgress.Value = Mathf.Clamp01(_reloadTimer / _currentWeapon.reloadTime);

        if (_reloadTimer >= _currentWeapon.reloadTime)
            _isReloading = false;
    }

    private void OnSelectionChanged(InventoryItemData selected)
    {
        if (_currentWeaponModel != null)
        {
            Destroy(_currentWeaponModel);
            _currentWeaponModel = null;
        }

        if (_bulletPool != null)
        {
            Destroy(_bulletPool.gameObject);
            _bulletPool = null;
        }

        _currentWeapon = selected as RangedWeaponItemData;

        _isReloading = false;
        if (reloadProgress != null) reloadProgress.Value = 1f;

        if (_currentWeapon == null) return;

        if (_currentWeapon.handItemPrefab != null && weaponSpawnPoint != null)
            _currentWeaponModel = Instantiate(_currentWeapon.handItemPrefab, weaponSpawnPoint.position, weaponSpawnPoint.rotation, weaponSpawnPoint);

        if (_currentWeapon.bulletPrefab != null)
            _bulletPool = BulletPool.Create(_currentWeapon.bulletPrefab, _currentWeapon.poolSize, transform);
    }

    public void Fire()
    {
        if (_currentWeapon == null || _bulletPool == null || _isReloading) return;
        if (!_bulletPool.TryGet(out Bullet bullet)) return;

        bullet.Launch(_bulletPool, _cam.transform.position, _cam.transform.forward,
            GetCurrentDamage(), _currentWeapon.bulletSpeed, _currentWeapon.maxDistance);

        if (fireSound != null) _audioSource.PlayOneShot(fireSound);

        StartReload();
    }

    private void StartReload()
    {
        _isReloading = true;
        _reloadTimer = 0f;
        if (reloadProgress != null) reloadProgress.Value = 0f;
        if (_currentWeapon.reloadSound != null) _audioSource.PlayOneShot(_currentWeapon.reloadSound);
    }

    private float GetCurrentDamage()
    {
        if (_currentWeapon.damageUpgrade != null && ItemUpgradeManager.Instance != null)
            return ItemUpgradeManager.Instance.GetCurrentValue(_currentWeapon.damageUpgrade);

        return _currentWeapon.baseDamage;
    }
}
