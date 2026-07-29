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
    private PlayerCamera _playerCamera;
    private PlayerMovement _playerMovement;
    private Camera _cam;

    private bool _isReloading;
    private float _reloadTimer;

    public bool IsAiming { get; private set; }

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        _playerCamera = GetComponentInChildren<PlayerCamera>();
        _playerMovement = GetComponent<PlayerMovement>();
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

    private bool IsBlocked => BlockPlayerMovement.Instance != null && BlockPlayerMovement.Instance.IsImmobilized;

    void Update()
    {
        IsAiming = !IsBlocked && _currentWeapon != null && _inputHandler != null && _inputHandler.AimHeld;
        _playerCamera?.SetAiming(IsAiming);
        _playerMovement?.SetAiming(IsAiming);

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
        if (IsBlocked || _currentWeapon == null || _bulletPool == null || _isReloading) return;
        if (!_bulletPool.TryGet(out Bullet bullet)) return;

        Vector3 origin = weaponSpawnPoint != null ? weaponSpawnPoint.position : _cam.transform.position;
        Vector3 direction = GetAimDirection(origin);
        bullet.Launch(_bulletPool, origin, direction,
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

    private Vector3 GetAimDirection(Vector3 origin)
    {
        Vector3 aimPoint = Physics.Raycast(_cam.transform.position, _cam.transform.forward, out RaycastHit hit, _currentWeapon.maxDistance)
            ? hit.point
            : _cam.transform.position + _cam.transform.forward * _currentWeapon.maxDistance;

        Vector3 toAimPoint = aimPoint - origin;

        if (Vector3.Dot(toAimPoint, _cam.transform.forward) <= 0.01f)
            return _cam.transform.forward;

        return toAimPoint.normalized;
    }

    private float GetCurrentDamage() => _currentWeapon.GetCurrentDamage();
}
