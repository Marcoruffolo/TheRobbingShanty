public readonly struct ZoneRange
{
    public readonly ZoneDefinition definition;
    public readonly float startZ;
    public readonly float endZ;

    public ZoneRange(ZoneDefinition definition, float startZ, float endZ)
    {
        this.definition = definition;
        this.startZ = startZ;
        this.endZ = endZ;
    }

    public bool Contains(float z) => z >= startZ && z < endZ;
}
