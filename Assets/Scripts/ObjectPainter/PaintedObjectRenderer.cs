using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[DisallowMultipleComponent]
public class PaintedObjectRenderer : MonoBehaviour
{
    private const int BatchSize = 1023;

    private readonly Dictionary<BatchKey, RenderBatch> batchesByKey = new();
    private readonly List<RenderBatch> batches = new();
    private readonly Dictionary<Material, Material> instancingMaterialCache = new();
    private readonly Matrix4x4[] matrixBuffer = new Matrix4x4[BatchSize];

    private PaintedObjectField field;
    private MaterialPropertyBlock propertyBlock;
    private bool dirty = true;

    public void MarkDirty()
    {
        dirty = true;
    }

    private void OnEnable()
    {
        field = GetComponent<PaintedObjectField>();
        propertyBlock ??= new MaterialPropertyBlock();
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
#if UNITY_EDITOR
        UnityEditor.SceneView.duringSceneGui += OnSceneViewGui;
#endif
        MarkDirty();
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
#if UNITY_EDITOR
        UnityEditor.SceneView.duringSceneGui -= OnSceneViewGui;
#endif
        ClearBatches();
        ClearInstancingMaterialCache();
    }

    private void OnValidate()
    {
        MarkDirty();
    }

    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
    {
        if (camera == null || field == null || field.Brush == null)
            return;

        if (dirty)
            RebuildBatches();

        DrawBatches(camera);
    }

#if UNITY_EDITOR
    private void OnSceneViewGui(UnityEditor.SceneView sceneView)
    {
        if (sceneView == null || Event.current == null || Event.current.type != EventType.Repaint)
            return;

        Camera sceneCamera = sceneView.camera;
        if (sceneCamera == null || field == null || field.Brush == null)
            return;

        if (dirty)
            RebuildBatches();

        DrawBatches(sceneCamera);
    }
#endif

    private void RebuildBatches()
    {
        dirty = false;
        ClearBatches();

        ObjectPainterBrush brush = field.Brush;
        IReadOnlyList<PaintedObjectInstance> instances = field.Instances;

        if (brush == null || instances == null || instances.Count == 0 || brush.variants == null)
            return;

        var variantRenderData = new ObjectPainterVariantRenderData[brush.variants.Count];
        for (int i = 0; i < brush.variants.Count; i++)
        {
            GameObject prefab = brush.variants[i]?.prefab;
            variantRenderData[i] = ObjectPainterVariantRenderData.Build(prefab);
        }

        for (int i = 0; i < instances.Count; i++)
        {
            PaintedObjectInstance instance = instances[i];
            if (instance.variantIndex < 0 || instance.variantIndex >= variantRenderData.Length)
                continue;

            ObjectPainterVariantRenderData renderData = variantRenderData[instance.variantIndex];
            if (renderData == null || renderData.parts.Count == 0)
                continue;

            Matrix4x4 instanceMatrix = Matrix4x4.TRS(instance.position, instance.rotation, instance.scale);

            for (int partIndex = 0; partIndex < renderData.parts.Count; partIndex++)
            {
                ObjectPainterRenderablePart part = renderData.parts[partIndex];
                if (part.mesh == null || part.material == null)
                    continue;

                Material material = GetInstancingMaterial(part.material);
                var key = new BatchKey(part.mesh, material, part.subMeshIndex, brush.outputLayer, brush.shadowCasting, brush.receiveShadows);

                if (!batchesByKey.TryGetValue(key, out RenderBatch batch))
                {
                    batch = new RenderBatch(key);
                    batchesByKey.Add(key, batch);
                    batches.Add(batch);
                }

                batch.matrices.Add(instanceMatrix * part.localMatrix);
            }
        }
    }

    private void DrawBatches(Camera camera)
    {
        for (int i = 0; i < batches.Count; i++)
        {
            RenderBatch batch = batches[i];
            if (batch.key.mesh == null || batch.key.material == null || batch.matrices.Count == 0)
                continue;

            for (int offset = 0; offset < batch.matrices.Count; offset += BatchSize)
            {
                int count = Mathf.Min(BatchSize, batch.matrices.Count - offset);
                for (int matrixIndex = 0; matrixIndex < count; matrixIndex++)
                {
                    matrixBuffer[matrixIndex] = batch.matrices[offset + matrixIndex];
                }

                Graphics.DrawMeshInstanced(
                    batch.key.mesh,
                    batch.key.subMeshIndex,
                    batch.key.material,
                    matrixBuffer,
                    count,
                    propertyBlock,
                    batch.key.shadowCasting,
                    batch.key.receiveShadows,
                    batch.key.layer,
                    camera,
                    LightProbeUsage.BlendProbes
                );
            }
        }
    }

    private Material GetInstancingMaterial(Material source)
    {
        if (source == null)
            return null;

        if (source.enableInstancing)
            return source;

        if (instancingMaterialCache.TryGetValue(source, out Material cached) && cached != null)
            return cached;

        var copy = new Material(source)
        {
            enableInstancing = true,
            hideFlags = HideFlags.HideAndDontSave
        };

        instancingMaterialCache[source] = copy;
        return copy;
    }

    private void ClearBatches()
    {
        batchesByKey.Clear();
        batches.Clear();
    }

    private void ClearInstancingMaterialCache()
    {
        foreach (Material material in instancingMaterialCache.Values)
        {
            DestroyMaterial(material);
        }

        instancingMaterialCache.Clear();
    }

    private static void DestroyMaterial(Material material)
    {
        if (material == null)
            return;

        if (Application.isPlaying)
            Destroy(material);
        else
            DestroyImmediate(material);
    }

    private readonly struct BatchKey : IEquatable<BatchKey>
    {
        public readonly Mesh mesh;
        public readonly Material material;
        public readonly int subMeshIndex;
        public readonly int layer;
        public readonly ShadowCastingMode shadowCasting;
        public readonly bool receiveShadows;

        public BatchKey(Mesh mesh, Material material, int subMeshIndex, int layer, ShadowCastingMode shadowCasting, bool receiveShadows)
        {
            this.mesh = mesh;
            this.material = material;
            this.subMeshIndex = subMeshIndex;
            this.layer = layer;
            this.shadowCasting = shadowCasting;
            this.receiveShadows = receiveShadows;
        }

        public bool Equals(BatchKey other)
        {
            return mesh == other.mesh &&
                   material == other.material &&
                   subMeshIndex == other.subMeshIndex &&
                   layer == other.layer &&
                   shadowCasting == other.shadowCasting &&
                   receiveShadows == other.receiveShadows;
        }

        public override bool Equals(object obj)
        {
            return obj is BatchKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = mesh != null ? mesh.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (material != null ? material.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ subMeshIndex;
                hashCode = (hashCode * 397) ^ layer;
                hashCode = (hashCode * 397) ^ (int)shadowCasting;
                hashCode = (hashCode * 397) ^ receiveShadows.GetHashCode();
                return hashCode;
            }
        }
    }

    private sealed class RenderBatch
    {
        public readonly BatchKey key;
        public readonly List<Matrix4x4> matrices = new();

        public RenderBatch(BatchKey key)
        {
            this.key = key;
        }
    }
}

internal sealed class ObjectPainterVariantRenderData
{
    public readonly List<ObjectPainterRenderablePart> parts = new();

    public static ObjectPainterVariantRenderData Build(GameObject prefab)
    {
        var data = new ObjectPainterVariantRenderData();
        if (prefab == null)
            return data;

        Transform root = prefab.transform;
        Matrix4x4 rootToLocal = root.worldToLocalMatrix;
        Matrix4x4 rootScale = Matrix4x4.Scale(root.localScale);

        MeshRenderer[] meshRenderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            MeshRenderer renderer = meshRenderers[i];
            MeshFilter filter = renderer.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null)
                continue;

            AddRendererParts(data, filter.sharedMesh, renderer.sharedMaterials, rootScale * (rootToLocal * renderer.transform.localToWorldMatrix));
        }

        SkinnedMeshRenderer[] skinnedRenderers = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < skinnedRenderers.Length; i++)
        {
            SkinnedMeshRenderer renderer = skinnedRenderers[i];
            if (renderer.sharedMesh == null)
                continue;

            AddRendererParts(data, renderer.sharedMesh, renderer.sharedMaterials, rootScale * (rootToLocal * renderer.transform.localToWorldMatrix));
        }

        return data;
    }

    private static void AddRendererParts(ObjectPainterVariantRenderData data, Mesh mesh, Material[] materials, Matrix4x4 localMatrix)
    {
        if (mesh == null || materials == null || materials.Length == 0)
            return;

        int subMeshCount = Mathf.Max(1, mesh.subMeshCount);
        for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
        {
            Material material = materials[Mathf.Min(subMeshIndex, materials.Length - 1)];
            if (material == null)
                continue;

            data.parts.Add(new ObjectPainterRenderablePart(mesh, material, subMeshIndex, localMatrix));
        }
    }
}

internal sealed class ObjectPainterRenderablePart
{
    public readonly Mesh mesh;
    public readonly Material material;
    public readonly int subMeshIndex;
    public readonly Matrix4x4 localMatrix;

    public ObjectPainterRenderablePart(Mesh mesh, Material material, int subMeshIndex, Matrix4x4 localMatrix)
    {
        this.mesh = mesh;
        this.material = material;
        this.subMeshIndex = subMeshIndex;
        this.localMatrix = localMatrix;
    }
}
