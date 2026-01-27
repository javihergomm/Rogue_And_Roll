using UnityEngine;

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
