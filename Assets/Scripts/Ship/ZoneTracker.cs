using UnityEngine;

public class ZoneTracker : MonoBehaviour
{
    [SerializeField] private NavigationRunState navigationRunState;

    private int _lastZoneIndex = -1;
    private NavigationRunState State => navigationRunState != null ? navigationRunState : NavigationRunState.Instance;

    private void Update()
    {
        ZoneLayoutResolver resolver = ZoneLayoutResolver.Instance;
        if (resolver == null) return;

        int zoneIndex = resolver.GetZoneIndexAt(transform.position.z);
        if (zoneIndex < 0 || zoneIndex == _lastZoneIndex) return;

        _lastZoneIndex = zoneIndex;
        State.SetCurrentZone(zoneIndex);
    }
}
