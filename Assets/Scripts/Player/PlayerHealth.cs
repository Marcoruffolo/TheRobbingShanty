using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("SOAP — Variables")]
    [SerializeField] private SOVariableFloat playerHealth;

    [Header("SOAP — Events")]
    [SerializeField] private VoidGameEvent onPlayerDeath;

    private bool _isDead;

    private void Start()
    {
        playerHealth.Value = 100f;
        _isDead = false;
    }

    public void TakeDamage(float amount)
    {
        if (_isDead) return;

        playerHealth.Value = Mathf.Max(0f, playerHealth.Value - amount);
        Debug.Log($"[PlayerHealth] HP: {playerHealth.Value}");

        if (playerHealth.Value <= 0f)
            Die();
    }

    private void Die()
    {
        _isDead = true;
        onPlayerDeath.Raise();
        Debug.Log("[PlayerHealth] Player muerto");
    }
}