using UnityEngine;

public class EnemyAnimationBridge : MonoBehaviour
{
    private EnemyAI _enemyAI;

    private void Awake()
    {
        _enemyAI = GetComponentInParent<EnemyAI>();
    }

    public void OnHitFrame()
    {
        _enemyAI.OnHitFrame();
    }

    public void OnAttackEnd()
    {
        _enemyAI.OnAttackEnd();
    }
}