using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private int swordItemID;
    [SerializeField] private GameObject swordModelPrefab;
    [SerializeField] private int axeItemID;
    [SerializeField] private GameObject axeModelPrefab;
    [SerializeField] private ItemUpgradeData swordDamageUpgrade;
    [SerializeField] private GameObject weaponHandModel;
    [SerializeField] private Transform weaponSpawnPoint;

    private GameObject _currentWeaponModel;

    [Header("Attacking")]
    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage = 1;
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
    private bool _swordEquipped;

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
        if (!_swordEquipped) return;
        if (!attacking) ChangeAnimationState(IDLE);
    }

    private void OnSelectionChanged(InventoryItemData selected)
    {
        if (_currentWeaponModel != null)
        {
            Destroy(_currentWeaponModel);
            _currentWeaponModel = null;
        }

        GameObject prefabToSpawn = null;
        if (selected != null && selected.ID == swordItemID) prefabToSpawn = swordModelPrefab;
        else if (selected != null && selected.ID == axeItemID) prefabToSpawn = axeModelPrefab;

        bool weaponEquipped = prefabToSpawn != null;
        if (weaponHandModel != null) weaponHandModel.SetActive(weaponEquipped);

        if (weaponEquipped && weaponSpawnPoint != null)
            _currentWeaponModel = Instantiate(prefabToSpawn, weaponSpawnPoint.position, weaponSpawnPoint.rotation, weaponSpawnPoint);

        _swordEquipped = selected != null && selected.ID == swordItemID;
        if (!_swordEquipped) attacking = false;
    }

    public void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }

    public void Attack()
    {
        if (!_swordEquipped || !readyToAttack || attacking) return;

        readyToAttack = false;
        attacking = true;

        Invoke(nameof(ResetAttack), attackSpeed);
        Invoke(nameof(AttackRaycast), attackDelay);

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
    }

    private float GetCurrentAttackDamage()
    {
        if (swordDamageUpgrade != null && ItemUpgradeManager.Instance != null)
            return ItemUpgradeManager.Instance.GetCurrentValue(swordDamageUpgrade);

        return attackDamage;
    }

    void AttackRaycast()
    {
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
