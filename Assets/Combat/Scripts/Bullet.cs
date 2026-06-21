using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifetime = 3f;

    private BulletPool _owner;
    private float _speed;
    private float _maxDistance;
    private float _damage;
    private Vector3 _startPosition;
    private float _elapsed;
    private bool _hasHit;

    public void Launch(BulletPool owner, Vector3 origin, Vector3 direction, float damage, float speed, float maxDistance)
    {
        _owner = owner;
        transform.SetParent(null, true);
        transform.SetPositionAndRotation(origin, Quaternion.LookRotation(direction));
        _damage = damage;
        _speed = speed;
        _maxDistance = maxDistance;
        _startPosition = origin;
        _elapsed = 0f;
        _hasHit = false;
    }

    private void Update()
    {
        transform.position += transform.forward * _speed * Time.deltaTime;
        _elapsed += Time.deltaTime;

        if (_elapsed >= lifetime || Vector3.Distance(_startPosition, transform.position) >= _maxDistance)
            ReturnToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_hasHit) return;
        _hasHit = true;

        IDamagable target = other.GetComponentInParent<IDamagable>();
        target?.TakeDamage(_damage);
        ReturnToPool();
    }

    private void ReturnToPool() => _owner.Return(this);
}
