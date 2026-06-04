using UnityEngine;

public static class LootGenerator
{
    public static void Generate(ChestLootTable table, InventorySystem target)
    {
        foreach (var entry in table.entries)
        {
            if (entry.item == null) continue;
            if (Random.value <= entry.probability)
            {
                int amount = Random.Range(entry.minAmount, entry.maxAmount + 1);
                target.AddToInventory(entry.item, amount);
            }
        }
    }
}
