using UnityEngine;

[CreateAssetMenu(fileName = "Knockback Config", menuName = "Combat/Knockback Config", order = 8)]
public class KnockbackConfigScriptableObject : ScriptableObject
{
    [SerializeField, Min(0f)] private float distance = 1.5f;
    [SerializeField, Min(0f)] private float displacementDuration = 0.25f;
    [SerializeField, Min(0f)] private float recoveryDuration = 0.75f;
    [SerializeField] private AnimationCurve movementCurve = new AnimationCurve(
        new Keyframe(0f, 0f),
        new Keyframe(0.25f, 0.55f),
        new Keyframe(0.6f, 0.9f),
        new Keyframe(1f, 1f));
    [SerializeField] private LayerMask obstacleMask;

    public LayerMask ObstacleMask => obstacleMask;

    public KnockbackRequest CreateRequest(Vector3 direction)
    {
        return new KnockbackRequest(
            direction,
            distance,
            displacementDuration,
            recoveryDuration,
            movementCurve,
            obstacleMask);
    }
}
