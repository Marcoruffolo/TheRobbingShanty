using UnityEngine;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : MonoBehaviour
{
    [Header("SOAP - Variables")]
    [SerializeField] private SOVariableFloat playerHealth;
    [SerializeField] private SOVariableFloat maxHealth;

    [SerializeField] GameObject bloodEffectPrefab;
    [SerializeField] ScriptableRendererFeature rendererFeature;
    [SerializeField] Material _fullscreenDamage;

    [Header("SOAP - Events")]
    [SerializeField] private VoidGameEvent onPlayerDeath;

    private bool _isDead;

    private void Start()
    {
        playerHealth.Value = maxHealth.Value;
        _isDead = false;
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        playerHealth.Value = Mathf.Max(0f, playerHealth.Value - amount);
        GameObject bloodEffect = Instantiate(bloodEffectPrefab, transform.position, Quaternion.identity);
        Destroy(bloodEffect, 6f);

        if (playerHealth.Value <= 0f)
            Die();
    }

    private void Die()
    {
        _isDead = true;
        onPlayerDeath.Raise();
    }
}
