using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectPainterBrush", menuName = "TRS/Object Painter Brush")]
public class ObjectPainterBrush : ScriptableObject
{
    [Header("Prefabs")]
    [Tooltip("Prefab variations used as visual sources. Meshes and materials are extracted for instanced rendering.")]
    public List<ObjectPainterPrefabVariant> variants = new();

    [Header("Brush")]
    [Min(0.1f)] public float brushRadius = 4f;
    [Min(1)] public int instancesPerStamp = 8;
    [Min(0f)] public float minSpacing = 0.75f;
    [Min(1)] public int maxInstancesPerStroke = 250;
    [Min(0.1f)] public float dragStampInterval = 0.08f;

    [Header("Placement")]
    public LayerMask surfaceMask = 1 << 3;
    [Min(0.1f)] public float projectionDistance = 30f;
    public float surfaceOffset = 0f;
    public bool alignToSurface = false;
    public bool randomYaw = true;
    [Min(0.01f)] public float minScale = 0.9f;
    [Min(0.01f)] public float maxScale = 1.2f;

    [Header("Output")]
    [Tooltip("Layer used by rendered instances and generated collider proxy objects.")]
    public int outputLayer = 0;
    public bool receiveShadows = true;
    public UnityEngine.Rendering.ShadowCastingMode shadowCasting = UnityEngine.Rendering.ShadowCastingMode.On;

    [Header("Collider Proxy")]
    [Tooltip("Enable only for sparse objects such as trees or rocks. Grass should usually keep this disabled.")]
    public bool createColliderProxies = false;
    public ObjectPainterColliderProxyType colliderProxyType = ObjectPainterColliderProxyType.Capsule;
    public Vector3 colliderCenter = new(0f, 1f, 0f);
    public Vector3 colliderSize = new(1f, 2f, 1f);
    public bool colliderIsTrigger = false;

    public bool HasValidVariants
    {
        get
        {
            if (variants == null)
                return false;

            for (int i = 0; i < variants.Count; i++)
            {
                if (variants[i] != null && variants[i].prefab != null && variants[i].weight > 0f)
                    return true;
            }

            return false;
        }
    }

    public int PickVariantIndex(float normalizedRandom)
    {
        if (variants == null || variants.Count == 0)
            return -1;

        float totalWeight = 0f;
        for (int i = 0; i < variants.Count; i++)
        {
            if (variants[i] != null && variants[i].prefab != null)
                totalWeight += Mathf.Max(0f, variants[i].weight);
        }

        if (totalWeight <= 0f)
            return -1;

        float target = Mathf.Clamp01(normalizedRandom) * totalWeight;
        float cumulative = 0f;

        for (int i = 0; i < variants.Count; i++)
        {
            if (variants[i] == null || variants[i].prefab == null)
                continue;

            cumulative += Mathf.Max(0f, variants[i].weight);
            if (target <= cumulative)
                return i;
        }

        for (int i = variants.Count - 1; i >= 0; i--)
        {
            if (variants[i] != null && variants[i].prefab != null && variants[i].weight > 0f)
                return i;
        }

        return -1;
    }

    private void OnValidate()
    {
        brushRadius = Mathf.Max(0.1f, brushRadius);
        instancesPerStamp = Mathf.Max(1, instancesPerStamp);
        minSpacing = Mathf.Max(0f, minSpacing);
        maxInstancesPerStroke = Mathf.Max(1, maxInstancesPerStroke);
        dragStampInterval = Mathf.Max(0.01f, dragStampInterval);
        projectionDistance = Mathf.Max(0.1f, projectionDistance);
        minScale = Mathf.Max(0.01f, minScale);
        maxScale = Mathf.Max(minScale, maxScale);
        outputLayer = Mathf.Clamp(outputLayer, 0, 31);
        colliderSize.x = Mathf.Max(0.01f, colliderSize.x);
        colliderSize.y = Mathf.Max(0.01f, colliderSize.y);
        colliderSize.z = Mathf.Max(0.01f, colliderSize.z);
    }
}

[System.Serializable]
public class ObjectPainterPrefabVariant
{
    public GameObject prefab;
    [Min(0f)] public float weight = 1f;
}

public enum ObjectPainterColliderProxyType
{
    None,
    Box,
    Capsule,
    Sphere
}
