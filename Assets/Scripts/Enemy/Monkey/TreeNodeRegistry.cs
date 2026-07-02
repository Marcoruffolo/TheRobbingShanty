using System.Collections.Generic;

public static class TreeNodeRegistry
{
    private static readonly List<TreeNodeAnchor> _nodes = new();

    public static IReadOnlyList<TreeNodeAnchor> All => _nodes;

    public static void Register(TreeNodeAnchor node)
    {
        if (_nodes.Contains(node)) return;
        _nodes.Add(node);
    }

    public static void Unregister(TreeNodeAnchor node) => _nodes.Remove(node);
}
