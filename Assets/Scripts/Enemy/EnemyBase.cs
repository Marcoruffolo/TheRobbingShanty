using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public abstract class EnemyBase : MonoBehaviour, IDamagable
{
    [Header("SOAP - Base")]
    [SerializeField] protected SOVariableFloat enemyHealth;
    [SerializeField] protected SOVariableFloat enemySpeed;
    [SerializeField] protected VoidGameEvent onEnemyDeath;

    public UnityAction OnDeath;
    public bool IsDead { get; private set; }
    public bool IsControlLocked { get; private set; }

    protected NavMeshAgent Agent;
    protected EnemyFOV Fov;
    protected Animator Animator;

    private float _currentHealth;

    protected static readonly int HashSpeed = Animator.StringToHash("Speed");
    protected static readonly int HashAttack = Animator.StringToHash("Attack");

    protected virtual void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Fov = GetComponent<EnemyFOV>();
        Animator = GetComponentInChildren<Animator>();
    }

    protected virtual void Start()
    {
        _currentHealth = enemyHealth.Value;
        Agent.speed = enemySpeed.Value;
    }

    public void TakeDamage(float damage)
    {
        if (IsDead) return;
        _currentHealth = Mathf.Max(0f, _currentHealth - damage);
        Debug.Log($"[{gameObject.name}] HP: {_currentHealth}");
        if (_currentHealth <= 0f) Die();
    }

    public void SetControlLocked(bool locked)
    {
        if (IsDead && locked) return;

        IsControlLocked = locked;

        if (Agent != null && Agent.enabled && Agent.isOnNavMesh)
        {
            Agent.isStopped = locked;

            if (locked)
                Agent.ResetPath();
        }

        if (!locked) return;

        if (Animator != null)
            Animator.SetFloat(HashSpeed, 0f);
    }

    public virtual void BeginKnockbackRecovery(float duration)
    {
        SetControlLocked(true);
    }

    public virtual void EndKnockbackRecovery()
    {
        SetControlLocked(false);
    }

    protected virtual void Die()
    {
        IsDead = true;
        GetComponent<EnemyRagdoll>()?.Activate();
        OnDeath?.Invoke();
    }

    public abstract void OnHitFrame();
    public abstract void OnAttackEnd();
}
