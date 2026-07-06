using UnityEngine;

public static class TreeConnectivity
{
    public static bool IsReachable(Vector3 from, Vector3 to, Transform fromTree, Transform toTree, float maxRange, LayerMask obstacleMask, bool ignoreOwnTree = true)
    {
        Vector3 offset = to - from;
        float distance = offset.magnitude;
        if (distance > maxRange) return false;

        bool sameTree = fromTree != null && fromTree == toTree;

        foreach (var hit in Physics.RaycastAll(from, offset.normalized, distance, obstacleMask))
        {
            if (!sameTree && ignoreOwnTree && (IsPartOf(hit.transform, fromTree) || IsPartOf(hit.transform, toTree))) continue;
            return false;
        }

        return true;
    }

    private static bool IsPartOf(Transform hit, Transform tree)
    {
        return tree != null && (hit == tree || hit.IsChildOf(tree));
    }
}
