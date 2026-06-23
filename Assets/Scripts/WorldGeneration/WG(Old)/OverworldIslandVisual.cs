using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class OverworldIslandVisual : MonoBehaviour
{
    [SerializeField] private Material islandMaterial;

    public IslandInstanceData InstanceData { get; private set; }

    public void Initialize(IslandInstanceData data, Material matOverride = null)
    {
        InstanceData = data;

        if (matOverride != null) islandMaterial = matOverride;

        BuildMesh(data.overworldRadius, data.seed);
    }

    private void BuildMesh(float radius, int seed)
    {
        int segments = 32;
        var verts = new Vector3[segments + 1];
        var tris = new int[segments * 3];

        // Centro elevado — da forma de isla y evita el problema de mesh plano
        verts[0] = new Vector3(0f, radius * 0.15f, 0f);

        for (int i = 0; i < segments; i++)
        {
            float angle = (float)i / segments * Mathf.PI * 2f;
            float noise = Mathf.PerlinNoise(
                Mathf.Cos(angle) * 0.5f + seed * 0.01f,
                Mathf.Sin(angle) * 0.5f + seed * 0.01f
            );
            float r = radius * Mathf.Lerp(0.78f, 1.22f, noise);

            // Borde al nivel del agua (Y=0), centro elevado → forma de isla natural
            verts[i + 1] = new Vector3(Mathf.Cos(angle) * r, 0f, Mathf.Sin(angle) * r);

            int ti = i * 3;
            tris[ti] = 0;
            tris[ti + 1] = (i + 1) % segments + 1;
            tris[ti + 2] = i + 1;
        }

        var mesh = new Mesh { name = "OverworldIsland" };
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.RecalculateNormals();

        var mf = GetComponent<MeshFilter>();
        var mr = GetComponent<MeshRenderer>();
        var mc = GetComponent<MeshCollider>();

        mf.sharedMesh = mesh;
        mc.sharedMesh = mesh;

        if (islandMaterial != null)
            mr.material = islandMaterial;
    }

    public void EnterIsland()
    {
        // En el futuro: IslandSceneLoader.PendingInstance = InstanceData;
        // SceneManager.LoadScene("IslandScene");
        Debug.Log($"[Overworld] Entrando a isla: {InstanceData.displayName}");
    }
}