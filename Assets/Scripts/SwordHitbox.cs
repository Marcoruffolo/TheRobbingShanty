using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    private Collider _col;

    private void Awake()
    {
        _col = GetComponent<Collider>();
        DisableHitbox();
    }

    public void EnableHitbox() => _col.enabled = true;
    public void DisableHitbox() => _col.enabled = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Enemy")) return;

        Debug.Log($"[SwordHitbox] Golpeó: {other.name}");
        DisableHitbox();
    }
}