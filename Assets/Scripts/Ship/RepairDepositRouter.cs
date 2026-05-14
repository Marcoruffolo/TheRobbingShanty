public interface IItemDepositHandler
{
    bool TryDeposit(InventorySlot sourceSlot);
}

public interface IBidirectionalDepositHandler : IItemDepositHandler
{
    bool TryDeposit(InventorySlot sourceSlot, InventorySystem sourceSystem);
}

public static class RepairDepositRouter
{
    private static IItemDepositHandler _activeHandler;

    public static bool HasActiveHandler => _activeHandler != null;

    public static void Register(IItemDepositHandler handler) => _activeHandler = handler;
    public static void Unregister() => _activeHandler = null;

    public static bool TryDeposit(InventorySlot sourceSlot, InventorySystem sourceSystem)
    {
        if (_activeHandler is IBidirectionalDepositHandler bidir)
            return bidir.TryDeposit(sourceSlot, sourceSystem);
        return _activeHandler != null && _activeHandler.TryDeposit(sourceSlot);
    }
}
