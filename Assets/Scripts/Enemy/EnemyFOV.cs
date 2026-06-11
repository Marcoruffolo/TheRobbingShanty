using UnityEngine;

public class EnemyFOV : MonoBehaviour
{
    private const string ObstacleTag = "Obstacle";

    [Header("Config")]
    [SerializeField] private float viewRadius = 10f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private LayerMask playerMask;

    public bool CanSeePlayer { get; private set; }
    public Transform PlayerTransform { get; private set; }

    private void Update() => DetectPlayer();

    private void DetectPlayer()
    {
        CanSeePlayer = false;
        PlayerTransform = null;

        Vector3 eyePos = transform.position + Vector3.up * 1.6f;
        Collider[] hits = Physics.OverlapSphere(eyePos, viewRadius, playerMask);

        foreach (var hit in hits)
        {
            Vector3 playerEye = hit.transform.position + Vector3.up * 1.6f;
            Vector3 dir = (playerEye - eyePos).normalized;
            float dist = Vector3.Distance(eyePos, playerEye);

            if (Vector3.Angle(transform.forward, dir) > viewAngle / 2f) continue;
            if (IsObstructed(eyePos, dir, dist)) continue;

            CanSeePlayer = true;
            PlayerTransform = hit.transform;
            return;
        }
    }

    private bool IsObstructed(Vector3 origin, Vector3 dir, float distance)
    {
        int ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        int mask = ~(1 << gameObject.layer | 1 << ignoreRaycastLayer);

        foreach (var hit in Physics.RaycastAll(origin, dir, distance, mask))
        {
            if (hit.collider.CompareTag(ObstacleTag))
                return true;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 eyePos = transform.position + Vector3.up * 1.6f;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePos, viewRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eyePos, DirFromAngle(-viewAngle / 2f) * viewRadius);
        Gizmos.DrawRay(eyePos, DirFromAngle(viewAngle / 2f) * viewRadius);
    }

    private Vector3 DirFromAngle(float angleDeg)
    {
        angleDeg += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleDeg * Mathf.Deg2Rad), 0f,
                           Mathf.Cos(angleDeg * Mathf.Deg2Rad));
    }
}