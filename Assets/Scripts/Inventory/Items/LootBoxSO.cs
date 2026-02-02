using System.Collections.Generic;
using UnityEngine;

/*
 * LootBoxSO
 * ---------
 * Loot box item that generates rewards dynamically at runtime.
 * Uses a dedicated LootBoxItemPool instead of the global ItemDatabase.
 * Only items with matching polarity are allowed.
 * Loot boxes cannot contain other loot boxes.
 */
[CreateAssetMenu(fileName = "LootBox", menuName = "Inventory/LootBox")]
public class LootBoxSO : BaseItemSO
{
    [Header("Loot Box Item Pool")]
    [SerializeField] private LootBoxItemPool lootPool;

    public override void UseItem()
    {
        BaseItemSO reward = Open();
        if (reward != null)
            LootBoxEvents.OnLootBoxOpened?.Invoke(this, reward);
    }

    public BaseItemSO Open()
    {
        List<BaseItemSO> candidates = GetValidRewards();

        if (candidates.Count == 0)
            return null;

        int index = Random.Range(0, candidates.Count);
        return candidates[index];
    }

    private List<BaseItemSO> GetValidRewards()
    {
        List<BaseItemSO> result = new List<BaseItemSO>();

        if (lootPool == null || lootPool.items == null)
            return result;

        foreach (var item in lootPool.items)
        {
            if (item == null)
                continue;

            // Do not allow loot boxes inside loot boxes
            if (item is LootBoxSO)
                continue;

            // Only allow items with the same polarity as this loot box
            if (item.Polarity == this.Polarity)
                result.Add(item);
        }

        return result;
    }
}
