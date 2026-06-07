using UnityEngine;

/*
 * LootBoxSO
 * ---------
 * Represents a loot box item that can be opened to obtain another item.
 * Loot boxes have two polarities:
 *   - Positive  -> rewards from the positive pool
 *   - Negative  -> rewards from the negative pool
 *
 * The loot box can:
 *   - Randomize its polarity when created
 *   - Force a specific polarity
 *   - Open itself and return a reward item
 *   - Unlock locked items based on unlockChance
 */
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

    // Public read-only access to the loot type
    public LootType Type => lootType;

    /*
     * Randomizes the loot box polarity (50/50).
     * Used when the loot box is created without a defined type.
     */
    public void RandomizePolarity()
    {
        lootType = (Random.value < 0.5f) ? LootType.Positive : LootType.Negative;
    }

    /*
     * Forces the loot box to a specific polarity.
     * Used when the game explicitly wants a Positive or Negative box.
     */
    public void ForcePolarity(LootType type)
    {
        lootType = type;
    }

    /*
     * Opens the loot box and returns a reward item.
     * The reward is selected from:
     *   - unlocked items
     *   - locked items (with chance to unlock)
     *
     * Unlock logic:
     *   1. If locked items exist -> chance to unlock one
     *   2. If unlocked items exist -> choose one
     *   3. If only locked items exist -> unlock one
     */
    public BaseItemSO Open()
    {
        BaseItemSO[] pool = (lootType == LootType.Positive)
            ? positiveItems
            : negativeItems;

        if (pool == null || pool.Length == 0)
        {
            return null;
        }

        var unlockedPool = new System.Collections.Generic.List<BaseItemSO>();
        var lockedPool = new System.Collections.Generic.List<BaseItemSO>();

        // Separate items into unlocked and locked pools
        foreach (var item in pool)
        {
            if (item == null)
                continue;

            if (Unlocks.IsUnlocked(item.itemID))
                unlockedPool.Add(item);
            else
                lockedPool.Add(item);
        }

        // 1. Chance to unlock a locked item
        if (lockedPool.Count > 0 && Random.value < unlockChance)
        {
            BaseItemSO reward = lockedPool[Random.Range(0, lockedPool.Count)];
            Unlocks.Unlock(reward.itemID);
            return reward;
        }

        // 2. Choose from unlocked items
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

    /*
     * Called when the player uses the loot box item.
     * Opens the box and dispatches the OnLootBoxOpened event.
     * InventoryManager should listen to this event to add the reward.
     */
    public override void UseItem()
    {
        BaseItemSO reward = Open();

        if (reward == null)
        {
            return;
        }

        // Notify listeners (InventoryManager should be subscribed)
        if (LootBoxEvents.OnLootBoxOpened != null)
            LootBoxEvents.OnLootBoxOpened(this, reward);
        else
            Debug.LogWarning("[LootBoxSO] No listeners for OnLootBoxOpened.");
    }
}

/*
 * LootBoxEvents
 * -------------
 * Global event used to notify when a loot box is opened.
 * InventoryManager must subscribe to this event to add the reward.
 */
public static class LootBoxEvents
{
    public static System.Action<LootBoxSO, BaseItemSO> OnLootBoxOpened;
}
