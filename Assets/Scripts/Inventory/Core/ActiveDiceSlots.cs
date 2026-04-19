using UnityEngine;
using System.Collections.Generic;

/*
 * ActiveDiceSlots
 * ----------------
 * Manages the list of active dice slots used by the player.
 * These slots represent dice currently equipped or active in the world.
 * The actual slot objects are owned and created by InventorySlots.
 *
 * This class provides:
 * - Initialization of active slots
 * - Queries such as checking if a slot is active or retrieving indices
 * - Synchronization between inventory slots and dice spawned in the world
 * - Helpers to get empty or non-empty slots
 */
[System.Serializable]
public class ActiveDiceSlots
{
    private List<ItemSlot> slots;

    // Initializes the active dice slot list with the provided slots
    public void Initialize(List<ItemSlot> activeSlots)
    {
        slots = activeSlots;
    }

    // Read-only access to the active slots
    public IReadOnlyList<ItemSlot> Slots => slots;

    // Returns true if the given slot is part of the active slot list
    public bool Contains(ItemSlot slot) => slots.Contains(slot);

    // Returns the index of the given slot inside the active slot list
    public int GetIndexOf(ItemSlot slot) => slots.IndexOf(slot);

    /*
     * SyncSlot
     * --------
     * Ensures that the world representation of a dice matches the state of the slot.
     * - If the slot is empty, remove the dice from the world.
     * - If the slot contains a dice item, spawn or update it in the world.
     */
    public void SyncSlot(ItemSlot slot)
    {
        if (!Contains(slot))
            return;

        if (slot.Quantity == 0)
        {
            DiceRollManager.Instance.RemoveDiceFromWorld(slot);
            return;
        }

        BaseItemSO item = slot.ItemSO;
        if (item is DiceSO dice)
            DiceRollManager.Instance.SpawnDiceInWorld(dice, slot);
    }

    /*
     * GetSelectedSlot
     * ----------------
     * Returns the first non-empty active slot.
     * Historically used to determine which dice is "selected",
     * but may no longer be used for movement logic.
     */
    public ItemSlot GetSelectedSlot()
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.Quantity > 0)
                return slot;
        }
        return null;
    }

    /*
     * GetFirstEmptySlot
     * ------------------
     * Returns the first active slot that is empty (Quantity == 0).
     */
    public ItemSlot GetFirstEmptySlot()
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.Quantity == 0)
                return slot;
        }
        return null;
    }

    /*
     * GetNonEmptySlots
     * -----------------
     * Returns all active slots that currently contain at least one item.
     * Useful when multiple dice can be active at the same time.
     */
    public IEnumerable<ItemSlot> GetNonEmptySlots()
    {
        foreach (var slot in slots)
        {
            if (slot != null && slot.Quantity > 0)
                yield return slot;
        }
    }
}
