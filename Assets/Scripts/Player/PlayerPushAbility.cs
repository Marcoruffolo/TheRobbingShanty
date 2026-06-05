using UnityEngine;

public class PlayerPushAbility : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private KnockbackConfigScriptableObject knockbackConfig;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Targeting")]
    [SerializeField] private float range = 2f;
    [Range(0f, 180f)]
    [SerializeField] private float coneAngle = 70f;
    [SerializeField] private Vector3 originOffset = new Vector3(0f, 1f, 0f);
    [SerializeField] private bool useCameraForward = true;
    [Min(1)]
    [SerializeField] private int maxTargets = 16;

    [Header("Timing")]
    [SerializeField] private float cooldown = 1f;

    private PlayerInputHandler _input;
    private Collider[] _hits;
    private Camera _camera;
    private bool _subscribed;
    private float _nextPushTime;

    private void Awake()
    {
        _hits = new Collider[Mathf.Max(1, maxTargets)];
    }

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

        if (!TryGetBestTarget(origin, forward, out IKnockbackable knockbackable, out Vector3 direction, out float distance))
            return;

        Vector3 force = knockbackConfig.GetKnockbackStrength(direction, distance);
        knockbackable.GetKnockedBack(force);
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

    private bool TryGetBestTarget(Vector3 origin, Vector3 forward, out IKnockbackable bestKnockbackable, out Vector3 bestDirection, out float bestDistance)
    {
        bestKnockbackable = null;
        bestDirection = Vector3.zero;
        bestDistance = 0f;

        float halfAngle = coneAngle * 0.5f;
        float bestScore = float.MaxValue;

        int hitCount = Physics.OverlapSphereNonAlloc(origin, range, _hits, enemyLayer, QueryTriggerInteraction.Collide);
        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = _hits[i];
            if (hit == null) continue;

            EnemyBase enemy = hit.GetComponentInParent<EnemyBase>();
            TrySetBestTarget(enemy, forward, halfAngle, ref bestScore, ref bestKnockbackable, ref bestDirection, ref bestDistance);
        }

        foreach (EnemyBase enemy in FindObjectsByType<EnemyBase>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            TrySetBestTarget(enemy, forward, halfAngle, ref bestScore, ref bestKnockbackable, ref bestDirection, ref bestDistance);
        }

        return bestKnockbackable != null;
    }

    private void TrySetBestTarget(
        EnemyBase enemy,
        Vector3 forward,
        float halfAngle,
        ref float bestScore,
        ref IKnockbackable bestKnockbackable,
        ref Vector3 bestDirection,
        ref float bestDistance)
    {
        if (enemy == null || enemy.IsDead) return;

        IKnockbackable knockbackable = enemy.GetComponent<IKnockbackable>();
        if (knockbackable == null) return;

        Vector3 direction = enemy.transform.position - transform.position;
        direction.y = 0f;
        float sqrDistance = direction.sqrMagnitude;
        if (sqrDistance <= 0.0001f) return;

        float distance = Mathf.Sqrt(sqrDistance);
        if (distance > range) return;

        Vector3 normalizedDirection = direction / distance;
        float angle = Vector3.Angle(forward, normalizedDirection);
        if (angle > halfAngle) return;

        float angleScore = halfAngle > 0f ? angle / halfAngle : 0f;
        float distanceScore = range > 0f ? distance / range : 0f;
        float score = angleScore + distanceScore;

        if (score >= bestScore) return;

        bestScore = score;
        bestKnockbackable = knockbackable;
        bestDirection = normalizedDirection;
        bestDistance = distance;
    }
}
