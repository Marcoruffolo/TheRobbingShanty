using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float hitRadius = 0.15f;
    [SerializeField] private LayerMask hitMask = ~0;

    private BulletPool _owner;
    private float _speed;
    private float _maxDistance;
    private float _damage;
    private Vector3 _startPosition;
    private float _elapsed;
    private bool _hasHit;
    private Rigidbody _rb;

    private void Awake() => _rb = GetComponent<Rigidbody>();

    public void Launch(BulletPool owner, Vector3 origin, Vector3 direction, float damage, float speed, float maxDistance)
    {
        _owner = owner;
        transform.SetParent(null, true);
        transform.SetPositionAndRotation(origin, Quaternion.LookRotation(direction));
        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _damage = damage;
        _speed = speed;
        _maxDistance = maxDistance;
        _startPosition = origin;
        _elapsed = 0f;
        _hasHit = false;
    }

    private void Update()
    {
        float step = _speed * Time.deltaTime;

        if (Physics.SphereCast(transform.position, hitRadius, transform.forward, out RaycastHit hit, step, hitMask, QueryTriggerInteraction.Collide))
        {
            transform.position = hit.point;
            HandleHit(hit.collider);
            return;
        }

        transform.position += transform.forward * step;
        _elapsed += Time.deltaTime;

        if (_elapsed >= lifetime || Vector3.Distance(_startPosition, transform.position) >= _maxDistance)
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other) => HandleHit(other);

    private void HandleHit(Collider other)
    {
        if (_hasHit) return;
        _hasHit = true;

        IDamagable target = other.GetComponentInParent<IDamagable>();
        target?.TakeDamage(_damage);
        ReturnToPool();
    }

    private void ReturnToPool() => _owner.Return(this);
}
