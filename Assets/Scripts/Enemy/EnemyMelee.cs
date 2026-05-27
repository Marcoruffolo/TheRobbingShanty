using Unity.VisualScripting;
using UnityEngine;

public class EnemyMelee : EnemyBase
{
    private enum State { Idle, Chase, Attack }

    [Header("SOAP Melee")]
    [SerializeField] private SOVariableFloat enemyDamage;

    [Header("Config")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float keepDistance = 3f;
    [SerializeField] private float cooldownMin = 0.3f;
    [SerializeField] private float cooldownMax = 0.8f;

    private PlayerHealth _playerHealth;
    private State _state = State.Idle;
    private float _nextAttackTime;


    protected override void Start()
    {
        base.Start();
        var player = GameObject.FindWithTag("Player");
        _playerHealth = player.GetComponent<PlayerHealth>();
        Agent.stoppingDistance = keepDistance;
        ScheduleNextAttack();
    }

    private void Update()
    {
        if (IsDead) return;
        switch (_state)
        {
            case State.Idle: UpdateIdle(); break;
            case State.Chase: UpdateChase(); break;
            case State.Attack: UpdateAttack(); break;
        }
    }

    private void UpdateIdle()
    {
        if (Animator == null) return;
        Animator.SetFloat(HashSpeed, 0f);
        if (Fov.CanSeePlayer) EnterChase();
    }

    private void UpdateChase()
    {
        if (!Agent.isOnNavMesh) return;
        if (!Fov.CanSeePlayer) { Agent.ResetPath(); _state = State.Idle; return; }

        float dist = Vector3.Distance(transform.position, Fov.PlayerTransform.position);
        if (dist <= attackRange) { EnterAttack(); return; }

        Agent.SetDestination(Fov.PlayerTransform.position);
        Animator.SetFloat(HashSpeed, Agent.velocity.magnitude);
    }

    private void UpdateAttack()
    {
        var info = Animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsTag("Attack") && info.normalizedTime < 1f) return;

        float dist = Vector3.Distance(transform.position, Fov.PlayerTransform.position);
        if (dist > attackRange * 1.2f) { _state = State.Chase; return; }
        FacePlayer();
        if (Time.time >= _nextAttackTime) DoAttack();
    }

    private void EnterChase()
    {
        _state = State.Chase;
        Agent.SetDestination(Fov.PlayerTransform.position);
    }

    private void EnterAttack()
    {
        Agent.ResetPath();
        Animator.SetFloat(HashSpeed, 0f);
        _state = State.Attack;
    }

    private void DoAttack()
    {
        FacePlayer();
        Animator.SetTrigger(HashAttack);
        ScheduleNextAttack();
    }

    public override void OnHitFrame()
    {
        float dist = Vector3.Distance(transform.position, Fov.PlayerTransform.position);
        if (dist <= attackRange)
            _playerHealth.TakeDamage(enemyDamage.Value);
    }

    public override void OnAttackEnd() { }
 

    private void FacePlayer()
    {
        Vector3 dir = (Fov.PlayerTransform.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private void ScheduleNextAttack()
    {
        _nextAttackTime = Time.time + Random.Range(cooldownMin, cooldownMax);
    }
}