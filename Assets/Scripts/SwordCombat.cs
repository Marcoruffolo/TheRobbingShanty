using UnityEngine;

public class SwordCombat : MonoBehaviour
{
    [Header("SOAP — Events")]
    [SerializeField] private VoidGameEvent onPlayerAttack;

    [Header("Referencias")]
    [SerializeField] private Animator swordAnimator;
    [SerializeField] private SwordHitbox swordHitbox;

    [Header("Config")]
    [SerializeField] private float attackCooldown = 0.8f;

    private PlayerInputHandler _input;
    private float _lastAttackTime = -999f;
    private bool _isAttacking;

    private void Awake()
    {
        _input = GetComponent<PlayerInputHandler>();
    }

    private void Start()
    {
        swordAnimator.ResetTrigger("Attack"); // limpia cualquier trigger acumulado
    }

    private void OnEnable()
    {
        Debug.Log("[PlayerCombat] Suscribiéndose a OnAttack");
        _input.OnAttack += HandleAttack;
    }

    private void OnDisable()
    {
        _input.OnAttack -= HandleAttack;
    }

    private void HandleAttack()
    {
        if (_isAttacking) return;
        if (Time.time - _lastAttackTime < attackCooldown) return;

        _lastAttackTime = Time.time;
        _isAttacking = true;

        Debug.Log($"[PlayerCombat] swordAnimator: {swordAnimator}");

        swordAnimator.SetTrigger("Attack");
    }
    public void OnHitFrame()
    {
        swordHitbox.EnableHitbox();
        onPlayerAttack.Raise();
    }

    public void OnAttackEnd()
    {
        swordHitbox.DisableHitbox();
        _isAttacking = false;
    }
}