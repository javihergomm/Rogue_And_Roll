using System.Collections.Generic;
using UnityEngine;

/*
 * LootBoxSO
 * ---------
 * Contains a pool of possible rewards.
 * When used, emits an event with the reward.
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
    [SerializeField] private LootType lootType;

    [Header("Possible Rewards")]
    [SerializeField] private BaseItemSO[] possibleItems;

    public LootType Type => lootType;

    public BaseItemSO Open()
    {
        if (possibleItems == null || possibleItems.Length == 0)
            return null;

        int index = Random.Range(0, possibleItems.Length);
        return possibleItems[index];
    }

    public override void UseItem()
    {
        BaseItemSO reward = Open();
        if (reward != null)
            LootBoxEvents.OnLootBoxOpened?.Invoke(this, reward);
    }

    public void ToggleType()
    {
        lootType = (lootType == LootType.Positive)
            ? LootType.Negative
            : LootType.Positive;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (possibleItems == null)
            return;

        List<BaseItemSO> valid = new List<BaseItemSO>();

        foreach (var item in possibleItems)
        {
            if (item == null)
                continue;

            bool boxPositive = lootType == LootType.Positive;
            bool itemPositive = item.Polarity == ItemPolarity.Positive;

            if (boxPositive == itemPositive)
                valid.Add(item);
        }

        possibleItems = valid.ToArray();
    }
#endif
}
