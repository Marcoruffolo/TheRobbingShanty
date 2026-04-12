using UnityEngine;
using UnityEngine.Localization.SmartFormat.PersistentVariables;

public class PlayerHealth : MonoBehaviour
{
    [Header("SOAP — Variables")]
    [SerializeField] private SOVariableFloat playerHealth;

    [Header("SOAP — Events")]
    [SerializeField] private FloatGameEvent onPlayerDamaged;
    [SerializeField] private VoidGameEvent onPlayerDeath;

    private void Start()
    {
        playerHealth.Value = 100f;
    }

    public void TakeDamage(float amount)
    {
        if (playerHealth.Value <= 0) return;

        playerHealth.Value = Mathf.Max(0f, playerHealth.Value - amount);
        onPlayerDamaged.Raise(playerHealth.Value);
        Debug.Log($"[PlayerHealth] HP: {playerHealth.Value}");

        if (playerHealth.Value <= 0)
            onPlayerDeath.Raise();
    }
}