using UnityEngine;
using UnityEngine.AI;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("SOAP - Base")]
    [SerializeField] protected SOVariableFloat enemyHealth;
    [SerializeField] protected SOVariableFloat enemySpeed;
    [SerializeField] protected VoidGameEvent onEnemyDeath;

    protected NavMeshAgent Agent;
    protected EnemyFOV Fov;
    protected Animator Animator;
    protected bool IsDead;

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
        IsDead = false;
    }

    public virtual void TakeDamage(float amount)
    {
        if (IsDead) return;
        _currentHealth = Mathf.Max(0f, _currentHealth - amount);
        Debug.Log($"[{gameObject.name}] HP: {_currentHealth}");
        if (_currentHealth <= 0f) Die();
    }

    protected virtual void Die()
    {
        IsDead = true;
        onEnemyDeath.Raise();
        //Destroy(gameObject, 10f);
    }

    public abstract void OnHitFrame();
    public abstract void OnAttackEnd();
}
