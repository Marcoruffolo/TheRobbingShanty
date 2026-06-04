using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    Animator animator;
    AudioSource audioSource;
    PlayerInputHandler inputHandler;
    Camera _cam;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
        _cam = Camera.main;
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
    }

    void OnDestroy()
    {
        if (inputHandler != null)
            inputHandler.OnAttack -= Attack;
    }

    void Update()
    {
        SetAnimations();
    }

    public const string IDLE = "Idle";
    public const string ATTACK1 = "Attack 1";
    public const string ATTACK2 = "Attack 2";

    string currentAnimationState;

    public void ChangeAnimationState(string newState)
    {
        if (currentAnimationState == newState) return;
        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }

    void SetAnimations()
    {
        if (!attacking)
            ChangeAnimationState(IDLE);
    }

    [Header("Attacking")]
    public float attackDistance = 3f;
    public float attackDelay = 0.4f;
    public float attackSpeed = 1f;
    public int attackDamage = 1;
    [SerializeField] private float hitRadius = 0.4f;

    public AudioClip swordSwing;
    public AudioClip hitSound;

    bool attacking = false;
    bool readyToAttack = true;
    int attackCount;

    public void Attack()
    {
        if (!readyToAttack || attacking) return;

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
