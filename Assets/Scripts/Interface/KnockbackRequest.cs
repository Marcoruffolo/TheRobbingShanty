using UnityEngine;

public readonly struct KnockbackRequest
{
    public Vector3 Direction { get; }
    public float Distance { get; }
    public float DisplacementDuration { get; }
    public float RecoveryDuration { get; }
    public AnimationCurve MovementCurve { get; }
    public LayerMask ObstacleMask { get; }

    public KnockbackRequest(
        Vector3 direction,
        float distance,
        float displacementDuration,
        float recoveryDuration,
        AnimationCurve movementCurve,
        LayerMask obstacleMask)
    {
        direction.y = 0f;
        Direction = direction.sqrMagnitude > 0f ? direction.normalized : Vector3.zero;
        Distance = Mathf.Max(0f, distance);
        DisplacementDuration = Mathf.Max(0f, displacementDuration);
        RecoveryDuration = Mathf.Max(0f, recoveryDuration);
        MovementCurve = movementCurve;
        ObstacleMask = obstacleMask;
    }

    public float EvaluateMovement(float normalizedTime)
    {
        float time = Mathf.Clamp01(normalizedTime);
        return Mathf.Clamp01(MovementCurve != null ? MovementCurve.Evaluate(time) : time);
    }
}
