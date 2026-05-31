using UnityEngine;

[CreateAssetMenu(fileName = "LootBox", menuName = "Inventory/LootBox")]
public class LootBoxSO : BaseItemSO
{
    public enum LootType
    {
        Positive,
        Negative
    }

    [Header("Loot Box Type (auto-assigned when obtained)")]
    [SerializeField] private LootType lootType;

    [Header("Positive Rewards")]
    [SerializeField] private BaseItemSO[] positiveItems;

    [Header("Negative Rewards")]
    [SerializeField] private BaseItemSO[] negativeItems;

    [Header("Chance to unlock a locked item (0 to 1)")]
    public float unlockChance = 0.2f;

    public LootType Type { get { return lootType; } }

    public void RandomizePolarity()
    {
        lootType = (Random.value < 0.5f) ? LootType.Positive : LootType.Negative;
    }

    public void ForcePolarity(LootType type)
    {
        lootType = type;
    }

    public BaseItemSO Open()
    {
        BaseItemSO[] pool = (lootType == LootType.Positive)
            ? positiveItems
            : negativeItems;

        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning("[LootBoxSO] LootBox '" + name + "' has no rewards for polarity " + lootType + ".");
            return null;
        }

        // Separate unlocked and locked items
        System.Collections.Generic.List<BaseItemSO> unlockedPool = new System.Collections.Generic.List<BaseItemSO>();
        System.Collections.Generic.List<BaseItemSO> lockedPool = new System.Collections.Generic.List<BaseItemSO>();

        foreach (var item in pool)
        {
            if (item == null)
                continue;

            if (Unlocks.IsUnlocked(item.itemID))
                unlockedPool.Add(item);
            else
                lockedPool.Add(item);
        }

        // 1. Chance to unlock a locked item even if unlocked items exist
        if (lockedPool.Count > 0 && Random.value < unlockChance)
        {
            BaseItemSO reward = lockedPool[Random.Range(0, lockedPool.Count)];
            Unlocks.Unlock(reward.itemID);
            return reward;
        }

        // 2. If there are unlocked items, choose one
        if (unlockedPool.Count > 0)
        {
            return unlockedPool[Random.Range(0, unlockedPool.Count)];
        }

        // 3. If no unlocked items exist, unlock one locked item
        if (lockedPool.Count > 0)
        {
            BaseItemSO reward = lockedPool[Random.Range(0, lockedPool.Count)];
            Unlocks.Unlock(reward.itemID);
            return reward;
        }

        return null;
    }

    public override void UseItem()
    {
        BaseItemSO reward = Open();

        if (reward == null)
        {
            Debug.LogWarning("[LootBoxSO] LootBox '" + name + "' returned NULL reward.");
            return;
        }

        if (LootBoxEvents.OnLootBoxOpened != null)
            LootBoxEvents.OnLootBoxOpened(this, reward);
        else
            Debug.LogWarning("[LootBoxSO] No listeners for OnLootBoxOpened.");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (positiveItems != null)
        {
            for (int i = 0; i < positiveItems.Length; i++)
            {
                BaseItemSO item = positiveItems[i];
                if (item != null && item.Polarity != ItemPolarity.Positive)
                {
                    Debug.LogWarning("[LootBoxSO] Removed invalid item '" + item.name +
                                     "' from Positive list in lootbox '" + name + "'.");
                    positiveItems[i] = null;
                }
            }
        }

        if (negativeItems != null)
        {
            for (int i = 0; i < negativeItems.Length; i++)
            {
                BaseItemSO item = negativeItems[i];
                if (item != null && item.Polarity != ItemPolarity.Negative)
                {
                    Debug.LogWarning("[LootBoxSO] Removed invalid item '" + item.name +
                                     "' from Negative list in lootbox '" + name + "'.");
                    negativeItems[i] = null;
                }
            }
        }
    }
#endif
}

public static class LootBoxEvents
{
    public static System.Action<LootBoxSO, BaseItemSO> OnLootBoxOpened;
}
