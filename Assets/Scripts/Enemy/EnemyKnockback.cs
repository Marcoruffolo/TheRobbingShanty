using UnityEngine;
using System.Collections;
using UnityEngine.AI;


public class EnemyKnockback : MonoBehaviour,IKnockbackable
{
    [SerializeField] private Collider physicsCollider;
    [SerializeField] private ForceMode forceMode = ForceMode.VelocityChange;
    [SerializeField] private float recoveryDelay = 1f;
    [SerializeField] private float maxKnockbackTime = 0.35f;
    [SerializeField] private float navMeshSampleRadius = 2f;

    private NavMeshAgent _agent;
    private Rigidbody _rigidbody;
    private EnemyBase _enemy;
    private Coroutine _knockbackCoroutine;
    private bool _physicsColliderDefaultEnabled;
    private bool _initialUseGravity;
    private bool _initialIsKinematic;
    private RigidbodyConstraints _initialConstraints;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _rigidbody = GetComponent<Rigidbody>();
        _enemy = GetComponent<EnemyBase>();

        if (physicsCollider != null)
            _physicsColliderDefaultEnabled = physicsCollider.enabled;

        _initialUseGravity = _rigidbody.useGravity;
        _initialIsKinematic = _rigidbody.isKinematic;
        _initialConstraints = _rigidbody.constraints;

        SetRigidbodyControlledByAgent();
    }

    public void GetKnockedBack(Vector3 force)
    {
        if ((_enemy != null && _enemy.IsDead) || force.sqrMagnitude <= 0.0001f) return;

        if (_knockbackCoroutine != null)
        {
            StopCoroutine(_knockbackCoroutine);
            _enemy?.EndKnockbackRecovery();
        }

        _knockbackCoroutine = StartCoroutine(ApplyKnockback(force));
    }

    public void StopForDeath()
    {
        if (_knockbackCoroutine != null)
            StopCoroutine(_knockbackCoroutine);

        _knockbackCoroutine = null;
        _enemy?.EndKnockbackRecovery();
        SetRigidbodyControlledByAgent();
    }

    private IEnumerator ApplyKnockback(Vector3 force)
    {
        yield return null;

        if (IsEnemyDead())
        {
            _knockbackCoroutine = null;
            yield break;
        }

        _enemy?.BeginKnockbackRecovery(recoveryDelay);

        if (_agent.enabled && _agent.isOnNavMesh)
            _agent.ResetPath();

        _agent.enabled = false;

        if (physicsCollider != null)
            physicsCollider.enabled = true;

        _rigidbody.useGravity = true;
        _rigidbody.isKinematic = false;
        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.AddForce(force, forceMode);

        yield return new WaitForSeconds(maxKnockbackTime);

        if (IsEnemyDead())
        {
            _knockbackCoroutine = null;
            yield break;
        }

        SetRigidbodyControlledByAgent();

        Vector3 agentPosition = transform.position;
        if (NavMesh.SamplePosition(agentPosition, out NavMeshHit hit, navMeshSampleRadius, NavMesh.AllAreas))
        {
            agentPosition = hit.position;
            transform.position = agentPosition;
        }

        _agent.enabled = true;
        if (_agent.isOnNavMesh)
        {
            _agent.Warp(agentPosition);
            _agent.ResetPath();
            _agent.isStopped = true;
        }

        _knockbackCoroutine = null;
    }

    private void SetRigidbodyControlledByAgent()
    {
        if (!_rigidbody.isKinematic)
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        _rigidbody.useGravity = _initialUseGravity;
        _rigidbody.isKinematic = _initialIsKinematic;
        _rigidbody.constraints = _initialConstraints;

        if (physicsCollider != null)
            physicsCollider.enabled = _physicsColliderDefaultEnabled;
    }

    private bool IsEnemyDead()
    {
        return _enemy != null && _enemy.IsDead;
    }
}
