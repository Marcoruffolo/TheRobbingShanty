using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(PaintedObjectRenderer))]
public class PaintedObjectField : MonoBehaviour
{
    private const string ColliderRootName = "_ObjectPainterColliderProxies";

    [SerializeField] private ObjectPainterBrush brush;
    [SerializeField] private List<PaintedObjectInstance> instances = new();
    [SerializeField, HideInInspector] private Transform colliderRoot;

#if UNITY_EDITOR
    private bool colliderRebuildQueued;
#endif

    public ObjectPainterBrush Brush => brush;
    public IReadOnlyList<PaintedObjectInstance> Instances => instances;
    public int InstanceCount => instances?.Count ?? 0;

    public void SetBrush(ObjectPainterBrush newBrush)
    {
        if (brush == newBrush)
            return;

        brush = newBrush;
        NotifyDataChanged(true);
    }

    public void AddInstance(PaintedObjectInstance instance)
    {
        instances ??= new List<PaintedObjectInstance>();
        instances.Add(instance);
        NotifyDataChanged(false);
    }

    public int RemoveInstancesInRadius(Vector3 center, float radius)
    {
        if (instances == null || instances.Count == 0)
            return 0;

        float sqrRadius = radius * radius;
        int removed = 0;

        for (int i = instances.Count - 1; i >= 0; i--)
        {
            if ((instances[i].position - center).sqrMagnitude > sqrRadius)
                continue;

            instances.RemoveAt(i);
            removed++;
        }

        if (removed > 0)
            NotifyDataChanged(false);

        return removed;
    }

    public bool HasInstanceWithin(Vector3 position, float radius)
    {
        if (instances == null || instances.Count == 0 || radius <= 0f)
            return false;

        float sqrRadius = radius * radius;
        for (int i = 0; i < instances.Count; i++)
        {
            if ((instances[i].position - position).sqrMagnitude <= sqrRadius)
                return true;
        }

        return false;
    }

    public void ClearInstances()
    {
        if (instances == null || instances.Count == 0)
            return;

        instances.Clear();
        NotifyDataChanged(true);
    }

    public void NotifyDataChanged(bool rebuildColliders)
    {
        MarkRendererDirty();

#if UNITY_EDITOR
        if (!Application.isPlaying)
            UnityEditor.EditorUtility.SetDirty(this);
#endif

        if (rebuildColliders)
            RebuildColliderProxies();
    }

    public void RebuildColliderProxies()
    {
        ResolveColliderRoot();

        bool needsColliders = brush != null &&
                              brush.createColliderProxies &&
                              brush.colliderProxyType != ObjectPainterColliderProxyType.None &&
                              instances != null &&
                              instances.Count > 0;

        if (!needsColliders)
        {
            DestroyColliderRoot();
            return;
        }

        if (colliderRoot == null)
        {
            var root = new GameObject(ColliderRootName);
            colliderRoot = root.transform;
            colliderRoot.SetParent(transform, false);
        }

        ClearColliderRootChildren();

        for (int i = 0; i < instances.Count; i++)
        {
            CreateColliderProxy(instances[i], i);
        }
    }

    private void OnEnable()
    {
        MarkRendererDirty();

        if (Application.isPlaying)
            RebuildColliderProxies();
    }

    private void OnValidate()
    {
        MarkRendererDirty();

#if UNITY_EDITOR
        if (!Application.isPlaying && !colliderRebuildQueued)
        {
            colliderRebuildQueued = true;
            UnityEditor.EditorApplication.delayCall += DelayedEditorColliderRebuild;
        }
#endif
    }

#if UNITY_EDITOR
    private void DelayedEditorColliderRebuild()
    {
        colliderRebuildQueued = false;

        if (this == null)
            return;

        RebuildColliderProxies();
        UnityEditor.SceneView.RepaintAll();
    }
#endif

    private void MarkRendererDirty()
    {
        var objectRenderer = GetComponent<PaintedObjectRenderer>();
        if (objectRenderer != null)
            objectRenderer.MarkDirty();
    }

    private void ResolveColliderRoot()
    {
        if (colliderRoot != null)
            return;

        var child = transform.Find(ColliderRootName);
        if (child != null)
            colliderRoot = child;
    }

    private void DestroyColliderRoot()
    {
        if (colliderRoot == null)
            return;

        DestroyGeneratedObject(colliderRoot.gameObject);
        colliderRoot = null;
    }

    private void ClearColliderRootChildren()
    {
        if (colliderRoot == null)
            return;

        for (int i = colliderRoot.childCount - 1; i >= 0; i--)
        {
            DestroyGeneratedObject(colliderRoot.GetChild(i).gameObject);
        }
    }

    private void CreateColliderProxy(PaintedObjectInstance instance, int index)
    {
        var proxy = new GameObject($"Collider Proxy {index:0000}");
        proxy.layer = Mathf.Clamp(brush.outputLayer, 0, 31);
        proxy.transform.SetParent(colliderRoot, false);
        proxy.transform.SetPositionAndRotation(instance.position, instance.rotation);
        proxy.transform.localScale = instance.scale;

        switch (brush.colliderProxyType)
        {
            case ObjectPainterColliderProxyType.Box:
            {
                var box = proxy.AddComponent<BoxCollider>();
                box.center = brush.colliderCenter;
                box.size = brush.colliderSize;
                box.isTrigger = brush.colliderIsTrigger;
                break;
            }
            case ObjectPainterColliderProxyType.Sphere:
            {
                var sphere = proxy.AddComponent<SphereCollider>();
                sphere.center = brush.colliderCenter;
                sphere.radius = Mathf.Max(brush.colliderSize.x, brush.colliderSize.z) * 0.5f;
                sphere.isTrigger = brush.colliderIsTrigger;
                break;
            }
            default:
            {
                var capsule = proxy.AddComponent<CapsuleCollider>();
                capsule.center = brush.colliderCenter;
                capsule.radius = Mathf.Max(brush.colliderSize.x, brush.colliderSize.z) * 0.5f;
                capsule.height = Mathf.Max(brush.colliderSize.y, capsule.radius * 2f);
                capsule.direction = 1;
                capsule.isTrigger = brush.colliderIsTrigger;
                break;
            }
        }
    }

    private static void DestroyGeneratedObject(Object target)
    {
        if (target == null)
            return;

        if (Application.isPlaying)
            Destroy(target);
        else
            DestroyImmediate(target);
    }
}

[System.Serializable]
public struct PaintedObjectInstance
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public Vector3 normal;
    public int variantIndex;

    public PaintedObjectInstance(Vector3 position, Quaternion rotation, Vector3 scale, Vector3 normal, int variantIndex)
    {
        this.position = position;
        this.rotation = rotation;
        this.scale = scale;
        this.normal = normal;
        this.variantIndex = variantIndex;
    }
}
