using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bomb : MonoBehaviour
{
    [Header("Explosión")]
    [SerializeField] private float explosionRadius = 3f;
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("Aterrizaje")]
    [SerializeField] private float armDelay = 0.1f;
    [SerializeField] private float maxGroundAngle = 45f;
    [SerializeField] private float detonationDelay = 2f;

    [Header("Titilar")]
    [SerializeField] private bool enableBlinking = true;
    [SerializeField] private float blinkInterval = 0.15f;

    private BombPool _owner;
    private float _damage;
    private float _fuseTimer;
    private float _armTimer;
    private float _detonationTimer;
    private float _blinkTimer;
    private LayerMask _damageMask;
    private bool _hasExploded;
    private bool _hasLanded;
    private Rigidbody _rb;
    private Renderer _renderer;
    private Collider _collider;
    private Transform _ignoredTree;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponentInChildren<Renderer>();
        _collider = GetComponent<Collider>();
    }

    public void Launch(BombPool owner, Vector3 origin, Vector3 velocity, float damage, float explosionRadiusOverride, float fuseTime, LayerMask damageMask, Transform ignoreTree = null)
    {
        _owner = owner;
        transform.SetParent(null, true);
        transform.position = origin;
        _rb.linearVelocity = velocity;
        _rb.angularVelocity = Vector3.zero;
        _damage = damage;
        explosionRadius = explosionRadiusOverride;
        _fuseTimer = fuseTime;
        _armTimer = armDelay;
        _damageMask = damageMask;
        _hasExploded = false;
        _hasLanded = false;
        if (_renderer != null) _renderer.enabled = true;
        SetIgnoredTree(ignoreTree);
    }

    private void SetIgnoredTree(Transform tree)
    {
        if (_ignoredTree == tree) return;

        if (_ignoredTree != null) SetIgnoreCollisions(_ignoredTree, false);
        _ignoredTree = tree;
        if (_ignoredTree != null) SetIgnoreCollisions(_ignoredTree, true);
    }

    private void SetIgnoreCollisions(Transform tree, bool ignore)
    {
        foreach (var col in tree.GetComponentsInChildren<Collider>())
            Physics.IgnoreCollision(_collider, col, ignore);
    }

    private void Update()
    {
        if (_armTimer > 0f) _armTimer -= Time.deltaTime;

        if (_hasLanded)
        {
            UpdateBlink();
            _detonationTimer -= Time.deltaTime;
            if (_detonationTimer <= 0f) Explode();
            return;
        }

        _fuseTimer -= Time.deltaTime;
        if (_fuseTimer <= 0f) Explode();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_armTimer > 0f || _hasLanded) return;

        Vector3 contactNormal = collision.GetContact(0).normal;
        if (Vector3.Angle(contactNormal, Vector3.up) <= maxGroundAngle)
            Land();
    }

    private void Land()
    {
        _hasLanded = true;
        _detonationTimer = detonationDelay;
        _blinkTimer = blinkInterval;
    }

    private void UpdateBlink()
    {
        if (!enableBlinking || _renderer == null) return;

        _blinkTimer -= Time.deltaTime;
        if (_blinkTimer <= 0f)
        {
            _blinkTimer = blinkInterval;
            _renderer.enabled = !_renderer.enabled;
        }
    }

    private void Explode()
    {
        if (_hasExploded) return;
        _hasExploded = true;

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius, _damageMask);
        foreach (var hit in hits)
        {
            IDamagable target = hit.GetComponentInParent<IDamagable>();
            target?.TakeDamage(_damage);
        }

        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }

        _owner.Return(this);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
