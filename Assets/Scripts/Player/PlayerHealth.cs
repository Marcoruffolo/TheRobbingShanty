using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("SOAP - Variables")]
    [SerializeField] private SOVariableFloat playerHealth;
    [SerializeField] private SOVariableFloat maxHealth;

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

        if (playerHealth.Value <= 0f)
            Die();
    }

    private void Die()
    {
        _isDead = true;
        onPlayerDeath.Raise();
    }
}
