using UnityEngine;

[CreateAssetMenu(fileName = "Knockback Config", menuName = "Combat/Knockback Config", order = 8)]
public class KnockbackConfigScriptableObject : ScriptableObject
{
    public float KnockbackStrength = 25000f;
    public ParticleSystem.MinMaxCurve DistanceFalloff = new ParticleSystem.MinMaxCurve(1f);

    public Vector3 GetKnockbackStrength(Vector3 direction, float distance)
    {
        return KnockbackStrength * DistanceFalloff.Evaluate(distance) * direction;
    }
}
