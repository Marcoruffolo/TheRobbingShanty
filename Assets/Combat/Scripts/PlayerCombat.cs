using UnityEngine;

[System.Serializable]
public class WeaponStats
{
    [Tooltip("Tiempo en segundos entre el inicio de un ataque y poder volver a atacar.")]
    public float attackSpeed = 1f;
    [Tooltip("Tiempo en segundos desde que empieza la animacion hasta que se aplica el golpe.")]
    public float attackDelay = 0.4f;
    public int attackDamage = 1;
    [Tooltip("Multiplicador de velocidad de la animacion de ataque (1 = normal, menos = mas lenta).")]
    public float animationSpeed = 1f;
}

public class PlayerCombat : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private GameObject weaponHandModel;
    [SerializeField] private Transform weaponSpawnPoint;

    private GameObject _currentWeaponModel;
    private WeaponItemData _currentWeapon;
    private bool _combatItemEquipped;

    [Header("Attacking")]
    public float attackDistance = 3f;
    [SerializeField] private float hitRadius = 0.4f;

    [SerializeField] GameObject hitEffectPrefab;
    public AudioClip swordSwing;
    public AudioClip hitSound;

    public const string IDLE = "Idle";
    public const string ATTACK1 = "Attack 1";
    public const string ATTACK2 = "Attack 2";

    private Animator animator;
    private AudioSource audioSource;
    private PlayerInputHandler inputHandler;
    private Camera _cam;
    private string currentAnimationState;
    private bool attacking;
    private bool readyToAttack = true;
    private int attackCount;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        _cam = Camera.main;
        if (weaponHandModel != null) weaponHandModel.SetActive(false);
    }

    void Start()
    {
        inputHandler = PlayerInputHandler.Instance;
        if (inputHandler == null)
        {
            Debug.LogError("[PlayerCombat] PlayerInputHandler instance not found.");
            return;
        }
        inputHandler.OnAttack += Attack;
        PlayerInventoryHolder.OnHotbarSelectionChanged += OnSelectionChanged;
    }

    void OnDestroy()
    {
        if (inputHandler != null) inputHandler.OnAttack -= Attack;
        PlayerInventoryHolder.OnHotbarSelectionChanged -= OnSelectionChanged;
    }

    void Update()
    {
        if (!_combatItemEquipped) return;
        if (!attacking) ChangeAnimationState(IDLE);
    }

    private void OnSelectionChanged(InventoryItemData selected)
    {
        if (_currentWeaponModel != null)
        {
            Destroy(_currentWeaponModel);
            _currentWeaponModel = null;
        }

        _currentWeapon = selected as WeaponItemData;
        _combatItemEquipped = selected is EquippableWeaponData;

        if (weaponHandModel != null) weaponHandModel.SetActive(_combatItemEquipped);

        if (_currentWeapon != null && _currentWeapon.handItemPrefab != null && weaponSpawnPoint != null)
            _currentWeaponModel = Instantiate(_currentWeapon.handItemPrefab, weaponSpawnPoint.position, weaponSpawnPoint.rotation, weaponSpawnPoint);

        if (!_combatItemEquipped) attacking = false;
    }

    public void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }

    public void Attack()
    {
        if (_currentWeapon == null || !readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;
        animator.speed = _currentWeapon.stats.animationSpeed;

        Invoke(nameof(ResetAttack), _currentWeapon.stats.attackSpeed);
        Invoke(nameof(AttackRaycast), _currentWeapon.stats.attackDelay);

        if (swordSwing != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(swordSwing);
        }

        if (attackCount == 0)
        {
            ChangeAnimationState(ATTACK1);
            attackCount++;
        }
        else
        {
            ChangeAnimationState(ATTACK2);
            attackCount = 0;
        }
    }

    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
        animator.speed = 1f;
    }

    private float GetCurrentAttackDamage()
    {
        if (_currentWeapon.damageUpgrade != null && ItemUpgradeManager.Instance != null)
            return ItemUpgradeManager.Instance.GetCurrentValue(_currentWeapon.damageUpgrade);

        return _currentWeapon.stats.attackDamage;
    }

    void AttackRaycast()
    {
        if (_currentWeapon == null) return;

        if (Physics.SphereCast(_cam.transform.position, hitRadius, _cam.transform.forward,
                               out RaycastHit hit, attackDistance))
        {
            IDamagable target = hit.transform.GetComponentInParent<IDamagable>();
            if (target != null)
            {
                target.TakeDamage(GetCurrentAttackDamage());
                if (hitSound != null)
                {
                    GameObject hitEffect = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(hitEffect, 0.5f);
                    audioSource.pitch = 1f;
                    audioSource.PlayOneShot(hitSound);
                }
            }
        }
    }
}
