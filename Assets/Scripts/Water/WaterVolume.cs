using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class WaterVolume : MonoBehaviour
{
    [Header("Water Surface")]
    [SerializeField] private float surfaceHeight = 0f;

    [Header("Gizmo")]
    [SerializeField] private Vector2 gizmoSize = new Vector2(20f, 20f);
    [SerializeField] private Color gizmoColor = new Color(0.2f, 0.6f, 1f, 0.35f);

    public float SurfaceY => transform.position.y + surfaceHeight;

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;

        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    private void OnDrawGizmos()
    {
        Vector3 center = new Vector3(transform.position.x, SurfaceY, transform.position.z);
        Vector3 size = new Vector3(gizmoSize.x, 0.01f, gizmoSize.y);

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(center, size);

        Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 1f);
        Gizmos.DrawWireCube(center, size);
    }
}
