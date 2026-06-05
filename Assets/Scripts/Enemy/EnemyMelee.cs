using UnityEngine;
using System.Collections;

public class EnemyMelee : EnemyBase
{
    private enum State { Idle, Chase, Attack, Recovery }

    [Header("SOAP Melee")]
    [SerializeField] private SOVariableFloat enemyDamage;

    [Header("Config")]
    [SerializeField] private float attackRange = 1.8f;
    [SerializeField] private float keepDistance = 3f;
    [SerializeField] private float attackDelay = 0.4f;
    [SerializeField] private float cooldownMin = 0.3f;
    [SerializeField] private float cooldownMax = 0.8f;

    private PlayerHealth _playerHealth;
    private State _state = State.Idle;
    private float _nextAttackTime;
    private Coroutine _recoveryCoroutine;
    private const string IdleStatePath = "Base Layer.Idle";
    private static readonly int HashIdleState = Animator.StringToHash(IdleStatePath);
    protected override void Start()
    {
        base.Start();
        var player = GameObject.FindWithTag("Player");
        if (player == null) { Debug.LogError("[EnemyMelee] No GameObject with tag 'Player' found."); return; }
        _playerHealth = player.GetComponent<PlayerHealth>();
        if (_playerHealth == null) Debug.LogError("[EnemyMelee] PlayerHealth not found on Player.");
        Agent.stoppingDistance = keepDistance;
        ScheduleNextAttack();
    }

    private void Update()
    {
        if (IsDead) return;
        switch (_state)
        {
            case State.Idle:  UpdateIdle();  break;
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
        if (!Agent.enabled || !Agent.isOnNavMesh) return;
        if (!Fov.CanSeePlayer) { Agent.ResetPath(); _state = State.Idle; return; }
        if (Fov.PlayerTransform == null) return;

        float dist = Vector3.Distance(transform.position, Fov.PlayerTransform.position);
        if (dist <= attackRange) { EnterAttack(); return; }

        Agent.isStopped = false;
        Agent.SetDestination(Fov.PlayerTransform.position);
        Animator.SetFloat(HashSpeed, Agent.velocity.magnitude);
    }

    private void UpdateAttack()
    {
        if (Fov.PlayerTransform == null) return;

        var info = Animator.GetCurrentAnimatorStateInfo(0);
        if (info.IsTag("Attack") && info.normalizedTime < 1f) return;

        float dist = Vector3.Distance(transform.position, Fov.PlayerTransform.position);
        if (dist > attackRange * 1.2f) { _state = State.Chase; return; }
        FacePlayer();
        if (Time.time >= _nextAttackTime) DoAttack();
    }

    private void EnterChase()
    {
        if (!Agent.enabled || !Agent.isOnNavMesh || Fov.PlayerTransform == null) return;

        _state = State.Chase;
        Agent.isStopped = false;
        Agent.SetDestination(Fov.PlayerTransform.position);
    }

    private void EnterAttack()
    {
        if (!Agent.enabled || !Agent.isOnNavMesh) return;

        StopAgent();
        Animator.SetFloat(HashSpeed, 0f);
        _state = State.Attack;
    }

    private void DoAttack()
    {
        FacePlayer();
        Animator.SetTrigger(HashAttack);
        Invoke(nameof(TryDealDamage), attackDelay);
        ScheduleNextAttack();
    }

    private void TryDealDamage()
    {
        if (IsDead || IsControlLocked || _playerHealth == null || Fov.PlayerTransform == null) return;
        float dist = Vector3.Distance(transform.position, Fov.PlayerTransform.position);
        if (dist <= attackRange)
            _playerHealth.TakeDamage(enemyDamage.Value);
    }

    public override void OnHitFrame()  { }
    public override void OnAttackEnd() { }

    private void FacePlayer()
    {
        if (Fov.PlayerTransform == null) return;
        Vector3 dir = (Fov.PlayerTransform.position - transform.position).normalized;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private void ScheduleNextAttack()
    {
        _nextAttackTime = Time.time + Random.Range(cooldownMin, cooldownMax);
    }

    private void StopAgent()
    {
        if (!Agent.enabled || !Agent.isOnNavMesh) return;

        Agent.isStopped = true;
        Agent.ResetPath();
    }


    public override void BeginKnockbackRecovery(float duration)
    {
        if (IsDead) return;

        if (_recoveryCoroutine != null)
            StopCoroutine(_recoveryCoroutine);

        CancelInvoke(nameof(TryDealDamage));
        ScheduleNextAttack();

        _state = State.Recovery;
        SetControlLocked(true);
        StopAgent();

        if (Animator != null)
        {
            Animator.ResetTrigger(HashAttack);
            Animator.SetFloat(HashSpeed, 0f);

            if (Animator.HasState(0, HashIdleState))
                Animator.CrossFade(HashIdleState, 0.05f, 0, 0f);
        }

        _recoveryCoroutine = StartCoroutine(KnockbackRecovery(duration));
    }

    public override void EndKnockbackRecovery()
    {
        if (_recoveryCoroutine != null)
        {
            StopCoroutine(_recoveryCoroutine);
            _recoveryCoroutine = null;
        }

        if (_state == State.Recovery)
            FinishRecovery();
    }

    private IEnumerator KnockbackRecovery(float duration)
    {
        yield return new WaitForSeconds(duration);
        _recoveryCoroutine = null;
        FinishRecovery();
    }

    private void FinishRecovery()
    {
        if (IsDead) return;

        SetControlLocked(false);

        if (Agent.enabled && Agent.isOnNavMesh)
            Agent.isStopped = false;

        _state = Fov.CanSeePlayer && Fov.PlayerTransform != null ? State.Chase : State.Idle;
    }
}
