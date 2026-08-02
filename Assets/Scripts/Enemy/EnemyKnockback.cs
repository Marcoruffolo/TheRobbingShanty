using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class EnemyKnockback : MonoBehaviour, IKnockbackable
{
    private NavMeshAgent _agent;
    private EnemyBase _enemy;
    private Coroutine _knockbackCoroutine;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _enemy = GetComponent<EnemyBase>();
    }

    private void OnEnable()
    {
        if (_enemy != null)
            _enemy.OnDeath += HandleDeath;
    }

    private void OnDisable()
    {
        if (_enemy != null)
            _enemy.OnDeath -= HandleDeath;

        if (_knockbackCoroutine == null) return;

        StopCoroutine(_knockbackCoroutine);
        _knockbackCoroutine = null;

        if (_enemy != null && !_enemy.IsDead)
            _enemy.EndKnockback();
    }

    public bool TryApplyKnockback(KnockbackRequest request)
    {
        if (_enemy == null || _enemy.IsDead ||
            _agent == null || !_agent.enabled || !_agent.isOnNavMesh ||
            request.Direction == Vector3.zero)
            return false;

        if (_knockbackCoroutine != null)
        {
            StopCoroutine(_knockbackCoroutine);
            _knockbackCoroutine = null;
        }

        _enemy.BeginKnockback();
        _knockbackCoroutine = StartCoroutine(ApplyKnockback(request));
        return true;
    }

    private IEnumerator ApplyKnockback(KnockbackRequest request)
    {
        Vector3 direction = request.Direction;
        Vector3 startPosition = transform.position;
        float allowedDistance = request.Distance;

        _agent.isStopped = true;
        _agent.ResetPath();

        if (_agent.Raycast(startPosition + direction * allowedDistance, out NavMeshHit navMeshHit))
        {
            float distanceToBoundary = Vector3.Dot(navMeshHit.position - startPosition, direction);
            allowedDistance = Mathf.Min(allowedDistance, Mathf.Max(0f, distanceToBoundary));
        }

        if (request.DisplacementDuration > 0f)
        {
            float elapsed = 0f;
            float movedDistance = 0f;

            while (elapsed < request.DisplacementDuration && movedDistance < allowedDistance)
            {
                elapsed = Mathf.Min(elapsed + Time.deltaTime, request.DisplacementDuration);
                float targetDistance = allowedDistance *
                    request.EvaluateMovement(elapsed / request.DisplacementDuration);
                float stepDistance = Mathf.Max(0f, targetDistance - movedDistance);

                bool blocked = TryMove(direction, stepDistance, request.ObstacleMask, out float appliedDistance);
                movedDistance += appliedDistance;

                if (blocked)
                    break;

                yield return null;
            }
        }
        else
        {
            TryMove(direction, allowedDistance, request.ObstacleMask, out _);
        }

        if (request.RecoveryDuration > 0f)
            yield return new WaitForSeconds(request.RecoveryDuration);

        _knockbackCoroutine = null;

        if (_enemy != null && !_enemy.IsDead)
            _enemy.EndKnockback();
    }

    private bool TryMove(
        Vector3 direction,
        float distance,
        LayerMask obstacleMask,
        out float appliedDistance)
    {
        appliedDistance = distance;
        if (distance <= 0f)
            return false;

        Vector3 center = transform.position +
            Vector3.up * (_agent.baseOffset + _agent.height * 0.5f);
        float capsuleOffset = Mathf.Max(0f, _agent.height * 0.5f - _agent.radius);
        Vector3 top = center + Vector3.up * capsuleOffset;
        Vector3 bottom = center - Vector3.up * capsuleOffset;

        bool blocked = Physics.CapsuleCast(
            top,
            bottom,
            _agent.radius,
            direction,
            out RaycastHit hit,
            distance,
            obstacleMask,
            QueryTriggerInteraction.Ignore);

        if (blocked)
            appliedDistance = Mathf.Min(distance, hit.distance);

        if (appliedDistance > 0f)
            _agent.Move(direction * appliedDistance);

        return blocked;
    }

    private void HandleDeath()
    {
        if (_knockbackCoroutine == null) return;

        StopCoroutine(_knockbackCoroutine);
        _knockbackCoroutine = null;
    }
}
