using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonkeyTreeHopper : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private float jumpRange = 8f;
    [SerializeField] private float jumpArcHeight = 2f;
    [SerializeField] private float jumpSpeed = 6f;
    [SerializeField] private float minJumpDuration = 0.25f;
    [SerializeField] private LayerMask obstacleMask;

    public TreeNodeAnchor CurrentNode { get; private set; }
    public bool IsJumping { get; private set; }

    private Transform _previousTree;

    public void SetInitialNode(TreeNodeAnchor node)
    {
        if (node == null || !node.TryReserve(this)) return;

        CurrentNode = node;
        transform.position = node.transform.position;
    }

    public bool TryFindDirectJumpTarget(out TreeNodeAnchor target)
    {
        if (TryFindRandomReachable(CurrentNode.transform.position, differentTreeOnly: true, excludeTree: _previousTree, out target))
            return true;

        return TryFindRandomReachable(CurrentNode.transform.position, differentTreeOnly: true, excludeTree: null, out target);
    }

    public bool TryFindRepositionTarget(out TreeNodeAnchor target)
    {
        target = null;
        if (CurrentNode == null) return false;

        var candidates = new List<TreeNodeAnchor>();

        foreach (var candidate in TreeNodeRegistry.All)
        {
            if (candidate == CurrentNode || candidate.OwnerTree != CurrentNode.OwnerTree || candidate.IsOccupied) continue;
            if (!TreeConnectivity.IsReachable(CurrentNode.transform.position, candidate.transform.position, CurrentNode.OwnerTree, candidate.OwnerTree, jumpRange, obstacleMask)) continue;
            if (!HasReachableDifferentTree(candidate)) continue;

            candidates.Add(candidate);
        }

        if (candidates.Count == 0) return false;

        target = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        return true;
    }

    public bool JumpTo(TreeNodeAnchor target, Action<bool> onJumpComplete)
    {
        if (IsJumping || target == null || !target.TryReserve(this)) return false;

        bool wasDifferentTree = CurrentNode == null || target.OwnerTree != CurrentNode.OwnerTree;
        if (wasDifferentTree && CurrentNode != null)
            _previousTree = CurrentNode.OwnerTree;

        CurrentNode?.Release(this);

        StartCoroutine(JumpRoutine(target, wasDifferentTree, onJumpComplete));
        return true;
    }

    private bool TryFindRandomReachable(Vector3 from, bool differentTreeOnly, Transform excludeTree, out TreeNodeAnchor target)
    {
        target = null;
        var candidates = new List<TreeNodeAnchor>();

        foreach (var candidate in TreeNodeRegistry.All)
        {
            if (candidate == CurrentNode || candidate.IsOccupied) continue;
            if (differentTreeOnly && candidate.OwnerTree == CurrentNode.OwnerTree) continue;
            if (excludeTree != null && candidate.OwnerTree == excludeTree) continue;

            float dist = Vector3.Distance(from, candidate.transform.position);
            if (dist > jumpRange) continue;

            if (!TreeConnectivity.IsReachable(from, candidate.transform.position, CurrentNode.OwnerTree, candidate.OwnerTree, jumpRange, obstacleMask)) continue;

            candidates.Add(candidate);
        }

        if (candidates.Count == 0) return false;

        target = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        return true;
    }

    private bool HasReachableDifferentTree(TreeNodeAnchor from)
    {
        Vector3 fromPos = from.transform.position;
        foreach (var candidate in TreeNodeRegistry.All)
        {
            if (candidate == from || candidate.OwnerTree == from.OwnerTree || candidate.IsOccupied) continue;
            if (TreeConnectivity.IsReachable(fromPos, candidate.transform.position, from.OwnerTree, candidate.OwnerTree, jumpRange, obstacleMask))
                return true;
        }
        return false;
    }

    private IEnumerator JumpRoutine(TreeNodeAnchor target, bool wasDifferentTree, Action<bool> onJumpComplete)
    {
        IsJumping = true;

        Vector3 from = transform.position;
        Vector3 to = target.transform.position;
        Vector3 flatDirection = to - from;
        flatDirection.y = 0f;
        if (flatDirection != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(flatDirection);

        float duration = Mathf.Max(Vector3.Distance(from, to) / jumpSpeed, minJumpDuration);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 flatPosition = Vector3.Lerp(from, to, t);
            float height = 4f * jumpArcHeight * t * (1f - t);
            transform.position = flatPosition + Vector3.up * height;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to;
        CurrentNode = target;
        IsJumping = false;
        onJumpComplete?.Invoke(wasDifferentTree);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, jumpRange);
    }
}
