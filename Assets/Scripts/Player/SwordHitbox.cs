using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    [Header("SOAP - Variables")]
    [SerializeField] private SOVariableFloat playerDamage;

    [Header("Config")]
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private LayerMask enemyLayer;

    private bool _isActive;
    private bool _hitThisSwing;

    public void EnableHitbox()
    {
        _isActive = true;
        _hitThisSwing = false;
    }

    public void DisableHitbox()
    {
        _isActive = false;
    }

    private void Update()
    {
        if (!_isActive || _hitThisSwing) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, radius, enemyLayer);

        foreach (var hit in hits)
        {
            EnemyBase enemy = hit.GetComponentInParent<EnemyBase>();
            if (enemy == null) continue;

            _hitThisSwing = true;
            enemy.TakeDamage(playerDamage.Value);
            Debug.Log($"[SwordHitbox] Golpeo a {hit.name} - daño: {playerDamage.Value}");
            break;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = _isActive ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
