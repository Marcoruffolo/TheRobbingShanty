using UnityEngine;
using System.Collections.Generic;

public class ZoneGateSpawner : MonoBehaviour
{
    [SerializeField] private GameObject gatePrefab;

    public void SpawnGates(IReadOnlyList<ZoneRange> zoneRanges, Transform parent)
    {
        if (zoneRanges == null || gatePrefab == null) return;

        for (int zoneIndex = 0; zoneIndex < zoneRanges.Count; zoneIndex++)
        {
            ZoneRange range = zoneRanges[zoneIndex];
            Vector3 position = new(range.definition.originX, 0f, range.endZ);
            GameObject gateObject = Instantiate(gatePrefab, position, Quaternion.identity, parent);
            gateObject.name = $"ZoneGate_{zoneIndex}";
            gateObject.GetComponentInChildren<ZoneGateController>()?.AssignZone(zoneIndex);
        }
    }
}
