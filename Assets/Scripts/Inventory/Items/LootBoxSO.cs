using System.Collections.Generic;
using UnityEngine;

/*
 * LootBoxSO
 * ---------
 * Loot box with dynamic polarity and separate reward tables.
 * Includes the event dispatcher inside the same file.
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

    public LootType Type { get { return lootType; } }

    // ---------------------------------------------------------
    // 1. Called when the player obtains the loot box
    // ---------------------------------------------------------
    public void RandomizePolarity()
    {
        lootType = (Random.value < 0.5f) ? LootType.Positive : LootType.Negative;
    }

    // ---------------------------------------------------------
    // 2. Select reward based on polarity
    // ---------------------------------------------------------
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

        int index = Random.Range(0, pool.Length);
        return pool[index];
    }

    // ---------------------------------------------------------
    // 3. UseItem triggers the event
    // ---------------------------------------------------------
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

}

/*
 * LootBoxEvents
 * -------------
 * Event dispatcher for loot box opening.
 * InventoryManager listens to this event to add the reward item.
 */
public static class LootBoxEvents
{
    public static System.Action<LootBoxSO, BaseItemSO> OnLootBoxOpened;
}
