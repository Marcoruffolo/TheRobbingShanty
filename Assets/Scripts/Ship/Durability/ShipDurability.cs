using UnityEngine;

public class ShipDurability : MonoBehaviour
{
    [SerializeField] private SOVariableFloat durability;
    [SerializeField] private float drainRatePerSecond = 1f;

    private void Update()
    {
        if (CameraModeController.Instance != null && CameraModeController.Instance.IsShipControlActive)
            durability.Add(-drainRatePerSecond * Time.deltaTime);
    }
}
