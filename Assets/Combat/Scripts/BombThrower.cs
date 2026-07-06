using UnityEngine;

public class BombThrower : MonoBehaviour
{
    [Header("Bomb")]
    [SerializeField] private Bomb bombPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private int poolSize = 4;
    [SerializeField] private LayerMask damageMask;

    [Header("Config")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private float fuseTime = 4f;
    [SerializeField] private float throwArcHeight = 3f;
    [SerializeField] private float fireRate = 2f;

    private BombPool _bombPool;
    private float _nextThrowTime;

    public bool CanThrow => Time.time >= _nextThrowTime;

    private void Awake()
    {
        if (bombPrefab != null)
            _bombPool = BombPool.Create(bombPrefab, poolSize, transform);
    }

    public void ThrowAt(Vector3 targetPosition, Transform ignoreTree = null)
    {
        if (!CanThrow || _bombPool == null || throwPoint == null) return;
        if (!_bombPool.TryGet(out Bomb bomb)) return;

        _nextThrowTime = Time.time + fireRate;

        Vector3 origin = transform.position + Vector3.up * throwPoint.localPosition.y;
        Vector3 velocity = CalculateArcVelocity(origin, targetPosition, throwArcHeight);
        bomb.Launch(_bombPool, origin, velocity, damage, explosionRadius, fuseTime, damageMask, ignoreTree, transform);
    }

    private static Vector3 CalculateArcVelocity(Vector3 origin, Vector3 target, float arcHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = target.y - origin.y;
        Vector3 displacementXZ = new Vector3(target.x - origin.x, 0f, target.z - origin.z);

        float time = Mathf.Sqrt(-2f * arcHeight / gravity) + Mathf.Sqrt(2f * (displacementY - arcHeight) / gravity);
        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2f * gravity * arcHeight);
        Vector3 velocityXZ = displacementXZ / time;

        return velocityXZ + velocityY * -Mathf.Sign(gravity);
    }
}
