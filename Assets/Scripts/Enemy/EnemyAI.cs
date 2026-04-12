using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class EnemyAI : MonoBehaviour
{
    private enum State { Idle, Chase, Attack }

    [Header("SOAP — Variables")]
    [SerializeField] private SOVariableFloat enemyDamage;
    [SerializeField] private SOVariableFloat enemySpeed;

    [Header("Config")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float cooldownMin = 0.3f;
    [SerializeField] private float cooldownMax = 0.8f;

    [Header("Referencias")]
    [SerializeField] private Animator animator;

    private NavMeshAgent _agent;
    private EnemyFOV _fov;
    private PlayerHealth _playerHealth;
    private State _state = State.Idle;
    private float _nextAttackTime;
    private bool _isAttacking;

    private static readonly int HashAttack = Animator.StringToHash("Attack");
    private static readonly int HashSpeed = Animator.StringToHash("Speed");

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _fov = GetComponent<EnemyFOV>();
    }

    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        _playerHealth = player.GetComponent<PlayerHealth>();
        _agent.speed = enemySpeed.Value;
        ScheduleNextAttack();
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Chase: UpdateChase(); break;
            case State.Attack: UpdateAttack(); break;
        }
    }

    private void UpdateIdle()
    {
        animator.SetFloat(HashSpeed, 0f);
        if (_fov.CanSeePlayer) EnterChase();
    }

    private void UpdateChase()
    {
        if (!_fov.CanSeePlayer)
        {
            _agent.ResetPath();
            _state = State.Idle;
            return;
        }

        float dist = Vector3.Distance(transform.position,
                                      _fov.PlayerTransform.position);
        if (dist <= attackRange) { EnterAttack(); return; }

        _agent.SetDestination(_fov.PlayerTransform.position);
        animator.SetFloat(HashSpeed, _agent.velocity.magnitude);
    }

    private void UpdateAttack()
    {
        if (_isAttacking) return;

        float dist = Vector3.Distance(transform.position,
                                      _fov.PlayerTransform.position);
        if (dist > attackRange * 1.2f) { _state = State.Chase; return; }

        FacePlayer();
        if (Time.time >= _nextAttackTime) DoAttack();
    }

    private void EnterChase()
    {
        _state = State.Chase;
        _agent.SetDestination(_fov.PlayerTransform.position);
    }

    private void EnterAttack()
    {
        _agent.ResetPath();
        animator.SetFloat(HashSpeed, 0f);
        _state = State.Attack;
    }

    private void DoAttack()
    {
        _isAttacking = true;
        FacePlayer();
        animator.SetTrigger(HashAttack);
    }

    public void OnHitFrame()
    {
        float dist = Vector3.Distance(transform.position,
                                      _fov.PlayerTransform.position);
        if (dist <= attackRange)
            _playerHealth.TakeDamage(enemyDamage.Value);
    }

    public void OnAttackEnd()
    {
        _isAttacking = false;
        ScheduleNextAttack();
    }

    private void FacePlayer()
    {
        Vector3 dir = (_fov.PlayerTransform.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private void ScheduleNextAttack()
    {
        _nextAttackTime = Time.time + Random.Range(cooldownMin, cooldownMax);
    }
}