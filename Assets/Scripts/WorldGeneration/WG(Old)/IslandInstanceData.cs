using UnityEngine;

public class IslandInstanceData : ScriptableObject
{
    public string instanceId;
    public string displayName;

    public Vector3 overworldPosition;
    public float overworldRadius;
    public bool isManuallyPlaced;
    public int seed;
    public float heightMultiplier = 8f;
    public float scale = 20f;
    public float waterLevel = 0.35f;
    public float beachLevel = 0.42f;
    public float forestLevel = 0.75f;
    public IslandSizeCategory sizeCategory;
}