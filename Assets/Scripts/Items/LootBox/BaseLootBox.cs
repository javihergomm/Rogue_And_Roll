using System.Collections.Generic;
using UnityEngine;

/*
 * LootBoxSO
 * ---------
 * A special item type that contains a pool of possible rewards.
 * When used, the loot box consumes itself and emits an event with the reward item.
 * It does not modify the inventory directly; external systems must handle adding the reward
 * and removing the loot box after use.
 */
[CreateAssetMenu(fileName = "LootBox", menuName = "Inventory/LootBox")]
public class LootBoxSO : BaseItemSO
{
    public enum LootType
    {
        Positive,
        Negative
    }

    [Header("Loot Box Type")]
    [SerializeField]
    private LootType lootType;

    [Header("Possible Rewards")]
    [SerializeField]
    private BaseItemSO[] possibleItems;

    public LootType Type => lootType;

    // Returns a random item from the pool
    public BaseItemSO Open()
    {
        if (possibleItems == null || possibleItems.Length == 0)
            return null;

        int index = Random.Range(0, possibleItems.Length);
        return possibleItems[index];
    }

    // Consumes the loot box and emits the reward
    public override void UseItem()
    {
        BaseItemSO reward = Open();

        if (reward != null)
            LootBoxEvents.OnLootBoxOpened?.Invoke(this, reward);
    }

    // Switches between Positive and Negative
    public void ToggleType()
    {
        lootType = (lootType == LootType.Positive)
            ? LootType.Negative
            : LootType.Positive;
    }

#if UNITY_EDITOR
    // Removes items whose polarity doesn't match the box type and compacts the array
    private void OnValidate()
    {
        if (possibleItems == null)
            return;

        List<BaseItemSO> validItems = new List<BaseItemSO>();

        foreach (var item in possibleItems)
        {
            if (item == null)
                continue;

            bool boxIsPositive = lootType == LootType.Positive;
            bool itemIsPositive = item.polarity == ItemPolarity.Positive;

            if (boxIsPositive == itemIsPositive)
            {
                validItems.Add(item);
            }
        }

        possibleItems = validItems.ToArray();
    }
#endif
}
