using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEnemySpawnSource
{
    event Action GenerationCompleted;
    bool HasGenerated { get; }
    IReadOnlyList<GameObject> SpawnedObjects { get; }
}
