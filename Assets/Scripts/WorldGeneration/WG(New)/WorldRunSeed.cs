using UnityEngine;

public static class WorldRunSeed
{
    private static int? _resolvedSeed;

    public static int Resolve(int configuredSeed)
    {
        if (configuredSeed != 0) return configuredSeed;

        _resolvedSeed ??= Random.Range(1, int.MaxValue);
        return _resolvedSeed.Value;
    }
}
