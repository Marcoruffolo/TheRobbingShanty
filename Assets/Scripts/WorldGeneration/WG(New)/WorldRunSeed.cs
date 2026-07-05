using UnityEngine;

public static class WorldRunSeed
{
    private static int? _resolvedSeed;

    public static int Resolve(int configuredSeed)
    {
        if (configuredSeed != 0)
        {
            _resolvedSeed = configuredSeed;
            return configuredSeed;
        }

        _resolvedSeed ??= Random.Range(1, int.MaxValue);
        return _resolvedSeed.Value;
    }

    public static void Reset()
    {
        _resolvedSeed = null;
    }
}
