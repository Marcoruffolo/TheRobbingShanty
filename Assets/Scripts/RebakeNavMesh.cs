using UnityEngine;
using Unity.AI.Navigation;

public class RebakeNavMesh : MonoBehaviour
{
    [SerializeField] private NavMeshSurface _navMeshSurface;
    [SerializeField] private ObjectProceduralGeneration _proceduralGeneration;

    void OnEnable()
    {
        _proceduralGeneration.GenerationCompleted += RebuildNavMesh;
    }

    void OnDisable()
    {
        _proceduralGeneration.GenerationCompleted -= RebuildNavMesh;
    }

    private void RebuildNavMesh()
    {
        Debug.Log("Navmesh");
        _navMeshSurface.BuildNavMesh();
    }
}
