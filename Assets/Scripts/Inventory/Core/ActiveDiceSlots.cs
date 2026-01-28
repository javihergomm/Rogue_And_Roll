using UnityEngine;
using System.Collections.Generic;

/*
 * ActiveDiceSlots
 * ---------------
 * Provides logic for active dice slots.
 * The actual slots are owned by InventorySlots.
 */
[System.Serializable]
public class ActiveDiceSlots
{
    private List<ItemSlot> slots;

    public void Initialize(List<ItemSlot> activeSlots)
    {
        slots = activeSlots;
    }

    public IReadOnlyList<ItemSlot> Slots => slots;

    public bool Contains(ItemSlot slot) => slots.Contains(slot);

    public int GetIndexOf(ItemSlot slot) => slots.IndexOf(slot);

    public void SyncSlot(ItemSlot slot)
    {
        if (!Contains(slot))
            return;

        if (slot.Quantity == 0)
        {
            DiceRollManager.Instance.RemoveDiceFromWorld(slot);
            return;
        }

        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.ItemName);
        if (item is DiceSO dice)
            DiceRollManager.Instance.SpawnDiceInWorld(dice, slot);
    }

    public ItemSlot GetSelectedSlot()
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.Quantity > 0)
                return slot;
        }
        return null;
    }

    public ItemSlot GetFirstEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.Quantity == 0)
                return slot;
        }
        return null;
    }
}
