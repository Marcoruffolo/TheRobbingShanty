using System.Collections.Generic;
using UnityEngine;

public class PlayerPushAbility : MonoBehaviour
{
    [SerializeField] private KnockbackConfigScriptableObject knockbackConfig;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float range = 2f;
    [Range(0f, 180f)]
    [SerializeField] private float coneAngle = 70f;
    [SerializeField] private Vector3 originOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private bool useCameraForward = true;
    [SerializeField] private float cooldown = 1f;

    private PlayerInputHandler _input;
    private Camera _camera;
    private readonly HashSet<EnemyBase> _candidates = new();
    private bool _subscribed;
    private float _nextPushTime;

    private void OnEnable()
    {
        TrySubscribe();
    }

    private void Start()
    {
        TrySubscribe();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (_subscribed) return;

        _input = PlayerInputHandler.Instance;
        if (_input == null) return;

        _input.OnPush += TryPush;
        _subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!_subscribed || _input == null) return;

        _input.OnPush -= TryPush;
        _subscribed = false;
    }

    private void TryPush()
    {
        if (knockbackConfig == null || Time.time < _nextPushTime) return;

        Vector3 origin = transform.position + originOffset;
        Vector3 forward = GetPushForward();

        if (!TryGetBestTarget(origin, forward, out IKnockbackable knockbackable, out Vector3 direction))
            return;

        if (!knockbackable.TryApplyKnockback(knockbackConfig.CreateRequest(direction)))
            return;

        _nextPushTime = Time.time + cooldown;
    }

    private Vector3 GetPushForward()
    {
        if (useCameraForward)
        {
            if (_camera == null)
                _camera = Camera.main;

            if (_camera != null)
            {
                Vector3 cameraForward = _camera.transform.forward;
                cameraForward.y = 0f;

                if (cameraForward.sqrMagnitude > 0.0001f)
                    return cameraForward.normalized;
            }
        }

        Vector3 forward = transform.forward;
        forward.y = 0f;
        return forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
    }

    private bool TryGetBestTarget(
        Vector3 origin,
        Vector3 forward,
        out IKnockbackable bestKnockbackable,
        out Vector3 bestDirection)
    {
        bestKnockbackable = null;
        bestDirection = Vector3.zero;

        float halfAngle = coneAngle * 0.5f;
        float bestScore = float.MaxValue;

        _candidates.Clear();
        Collider[] hits = Physics.OverlapSphere(origin, range, enemyLayer, QueryTriggerInteraction.Collide);
        foreach (Collider hit in hits)
        {
            if (hit == null) continue;

            EnemyBase enemy = hit.GetComponentInParent<EnemyBase>();
            if (enemy == null || !_candidates.Add(enemy)) continue;

            TrySetBestTarget(
                enemy,
                origin,
                forward,
                halfAngle,
                ref bestScore,
                ref bestKnockbackable,
                ref bestDirection);
        }

        return bestKnockbackable != null;
    }

    private void TrySetBestTarget(
        EnemyBase enemy,
        Vector3 origin,
        Vector3 forward,
        float halfAngle,
        ref float bestScore,
        ref IKnockbackable bestKnockbackable,
        ref Vector3 bestDirection)
    {
        if (enemy == null || enemy.IsDead) return;

        IKnockbackable knockbackable = enemy.GetComponent<IKnockbackable>();
        if (knockbackable == null) return;

        Collider targetCollider = enemy.GetComponent<Collider>();
        if (targetCollider == null) return;

        Vector3 rangePoint = targetCollider.ClosestPoint(origin);
        Vector3 rangeOffset = rangePoint - origin;
        rangeOffset.y = 0f;
        float rangeDistance = rangeOffset.magnitude;
        if (rangeDistance > range) return;

        Vector3 colliderCenter = targetCollider.bounds.center;
        colliderCenter.y = origin.y;
        float axisDistance = Mathf.Clamp(
            Vector3.Dot(colliderCenter - origin, forward),
            0f,
            range);
        Vector3 targetPoint =
            targetCollider.ClosestPoint(origin + forward * axisDistance);
        Vector3 targetOffset = targetPoint - origin;
        targetOffset.y = 0f;
        float targetDistance = targetOffset.magnitude;
        if (targetDistance > range) return;

        float angle = 0f;

        if (targetDistance > 0.0001f)
        {
            Vector3 targetDirection = targetOffset / targetDistance;
            angle = Vector3.Angle(forward, targetDirection);
            if (angle > halfAngle) return;
        }

        if (!HasClearLineOfEffect(origin, targetPoint, enemy)) return;

        Vector3 knockbackDirection = enemy.transform.position - transform.position;
        knockbackDirection.y = 0f;
        float knockbackSqrDistance = knockbackDirection.sqrMagnitude;
        if (knockbackSqrDistance <= 0.0001f) return;

        Vector3 normalizedKnockbackDirection =
            knockbackDirection / Mathf.Sqrt(knockbackSqrDistance);

        float angleScore = halfAngle > 0f ? angle / halfAngle : 0f;
        float distanceScore = range > 0f ? rangeDistance / range : 0f;
        float score = angleScore + distanceScore;

        if (score >= bestScore) return;

        bestScore = score;
        bestKnockbackable = knockbackable;
        bestDirection = normalizedKnockbackDirection;
    }

    private bool HasClearLineOfEffect(
        Vector3 origin,
        Vector3 targetPoint,
        EnemyBase target)
    {
        Vector3 direction = targetPoint - origin;
        float distance = direction.magnitude;
        if (distance <= 0.0001f) return true;

        RaycastHit[] hits = Physics.RaycastAll(
            origin,
            direction / distance,
            distance,
            knockbackConfig.ObstacleMask,
            QueryTriggerInteraction.Ignore);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == null || hit.collider.transform.IsChildOf(transform))
                continue;

            EnemyBase hitEnemy = hit.collider.GetComponentInParent<EnemyBase>();
            if (hitEnemy == target) continue;

            return false;
        }

        return true;
    }
}
