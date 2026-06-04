using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private InventoryItemData swordItemData;
    [SerializeField] private GameObject swordModel;

    [Header("Attacking")]
    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage = 1;
    [SerializeField] private float hitRadius = 0.4f;

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
        if (swordModel != null) swordModel.SetActive(false);
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
        _swordEquipped = selected == swordItemData;
        if (swordModel != null) swordModel.SetActive(_swordEquipped);
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

    void AttackRaycast()
    {
        if (Physics.SphereCast(_cam.transform.position, hitRadius, _cam.transform.forward,
                               out RaycastHit hit, attackDistance))
        {
            IDamagable target = hit.transform.GetComponentInParent<IDamagable>();
            if (target != null)
            {
                target.TakeDamage(attackDamage);
                if (hitSound != null)
                {
                    audioSource.pitch = 1f;
                    audioSource.PlayOneShot(hitSound);
                }
            }
        }
    }
}
