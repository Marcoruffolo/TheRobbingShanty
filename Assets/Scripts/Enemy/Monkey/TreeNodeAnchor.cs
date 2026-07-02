using UnityEngine;

public class TreeNodeAnchor : MonoBehaviour
{
    public Transform OwnerTree
    {
        get
        {
            Transform t = transform.parent;
            while (t != null && t.GetComponent<Collider>() == null)
                t = t.parent;
            return t != null ? t : transform.parent;
        }
    }

    public MonkeyTreeHopper Occupant { get; private set; }
    public bool IsOccupied => Occupant != null;

    private void OnEnable() => TreeNodeRegistry.Register(this);
    private void OnDisable() => TreeNodeRegistry.Unregister(this);

    public bool TryReserve(MonkeyTreeHopper hopper)
    {
        if (IsOccupied && Occupant != hopper) return false;
        Occupant = hopper;
        return true;
    }

    public void Release(MonkeyTreeHopper hopper)
    {
        if (Occupant == hopper) Occupant = null;
    }
}
