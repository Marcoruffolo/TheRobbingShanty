using UnityEngine;

public class EnemyAnimationBridge : MonoBehaviour
{
    private EnemyBase _enemy;

    private void Awake() => _enemy = GetComponentInParent<EnemyBase>();

    public void OnHitFrame() => _enemy.OnHitFrame();
    public void OnAttackEnd() => _enemy.OnAttackEnd();
}