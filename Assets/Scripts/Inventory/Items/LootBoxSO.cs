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

    // Called when the player obtains the loot box
    public void RandomizePolarity()
    {
        lootType = (Random.value < 0.5f) ? LootType.Positive : LootType.Negative;
    }

    // Used by Movement when falling on Good/Bad spots
    public void ForcePolarity(LootType type)
    {
        lootType = type;
    }

    // Select reward based on polarity (single check)
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

        return pool[Random.Range(0, pool.Length)];
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
        // Enforce correct polarity in Positive list
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

        // Enforce correct polarity in Negative list
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
