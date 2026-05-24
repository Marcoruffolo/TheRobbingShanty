using UnityEngine;

public class ShipDurability : MonoBehaviour
{
    [SerializeField] private SOVariableFloat durability;
    [SerializeField] private Material energyMaterial;
    [SerializeField] private float drainRatePerSecond = 1f;

    private void Update()
    {
        energyMaterial.SetFloat("_Durability", durability.Value);
        if (CameraModeController.Instance != null && CameraModeController.Instance.IsShipControlActive)
            durability.Add(-drainRatePerSecond * Time.deltaTime);

    }
}
