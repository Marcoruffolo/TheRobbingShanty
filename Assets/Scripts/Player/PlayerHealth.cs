using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class PlayerHealth : MonoBehaviour
{
    [Header("SOAP - Variables")]
    [SerializeField] private SOVariableFloat playerHealth;
    [SerializeField] private SOVariableFloat maxHealth;

    [SerializeField] GameObject bloodEffectPrefab;
    [SerializeField] ScriptableRendererFeature rendererFeature;
    [SerializeField] Material _fullscreenDamage;
    [SerializeField] float fadeSpeed = 3f;
    [SerializeField] float maxAlpha = 2f;
    float currentAlpha;


    [Header("SOAP - Events")]
    [SerializeField] private VoidGameEvent onPlayerDeath;

    private bool _isDead;

    void Awake()
    {
        rendererFeature.SetActive(false);
    }

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

    void Update()
    {
        float healthPercent = (float) playerHealth.Value / maxHealth.Value;
        float quarterHealth = 0.45f;

        float targetAlpha = 0f;

        if (healthPercent <= quarterHealth)
        {
            rendererFeature.SetActive(true);
            float intensity = 1f - (healthPercent / quarterHealth);
            targetAlpha = intensity * maxAlpha;
        }

        // Smooth transition
        currentAlpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * fadeSpeed);

        _fullscreenDamage.SetFloat("_Strength", currentAlpha);

        // Disable when fully invisible
        if (currentAlpha <= 0.01f && healthPercent > quarterHealth)
            rendererFeature.SetActive(false);
    }
}
