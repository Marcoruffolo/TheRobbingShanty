using UnityEngine;

public class SwordAnimationBridge : MonoBehaviour
{
    private PlayerCombat _playerCombat;

    private void Awake()
    {      
        _playerCombat = GetComponentInParent<PlayerCombat>();

        if (_playerCombat == null)
            Debug.LogError("[SwordAnimationBridge] No encontró PlayerCombat.");
    }

    public void OnHitFrame()
    {
        Debug.Log("[Bridge] Evento Hit recibido");
        _playerCombat?.OnHitFrame();
    }

    public void OnAttackEnd()
    {
        Debug.Log("[Bridge] Evento Fin Ataque recibido");
        _playerCombat?.OnAttackEnd();
    }
}