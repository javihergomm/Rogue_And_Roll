using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/*
 * InventorySlots
 * --------------
 * Logical grouping and management of all inventory slots.
 * Handles:
 *   - Slot categories (ActiveDice, Dice, Permanent, Consumable)
 *   - Adding/removing items
 *   - Click behavior for items (Dice, Permanents, Consumables, LootBoxes)
 *   - Replace mode (selecting a target slot for an item/effect)
 */
[System.Serializable]
public class InventorySlots
{
    [Header("Slot Groups")]
    [SerializeField] private List<ItemSlot> activeDiceSlots;
    [SerializeField] private List<ItemSlot> diceSlots;
    [SerializeField] private List<ItemSlot> permanentSlots;
    [SerializeField] private List<ItemSlot> consumableSlots;

    [Header("Item Database")]
    [SerializeField] private BaseItemSO[] itemSOs;

    private Dictionary<string, BaseItemSO> lookup;
    private readonly List<ItemSlot> allSlots = new();

    public IReadOnlyList<ItemSlot> AllSlots => allSlots;
    public List<ItemSlot> ActiveDiceSlots => activeDiceSlots;

    /*
     * Initializes slot types and builds the item lookup.
     * Must be called once from InventoryManager.Awake().
     */
    public void Initialize()
    {
        lookup = new Dictionary<string, BaseItemSO>();
        foreach (var item in itemSOs)
            lookup[item.ItemName] = item;

        allSlots.Clear();

        // Assign slot types and register in allSlots
        foreach (var s in activeDiceSlots)
        {
            s.SetSlotType(SlotType.ActiveDice);
            allSlots.Add(s);
        }

        foreach (var s in diceSlots)
        {
            s.SetSlotType(SlotType.Dice);
            allSlots.Add(s);
        }

        foreach (var s in permanentSlots)
        {
            s.SetSlotType(SlotType.Permanent);
            allSlots.Add(s);
        }

        foreach (var s in consumableSlots)
        {
            s.SetSlotType(SlotType.Consumable);
            allSlots.Add(s);
        }
    }

    /*
     * Returns an item ScriptableObject by name.
     * Delegates to InventoryManager's catalog.
     */
    public BaseItemSO GetItemSO(string name)
    {
        return InventoryManager.Instance.GetItemSO(name);
    }

    /*
     * Adds an item to the appropriate category slots.
     * Stacks with same item if possible, otherwise shows "inventory full" popup.
     */
    public void AddItem(BaseItemSO item, int qty)
    {
        List<ItemSlot> target = GetCategory(item);

        foreach (var slot in target)
        {
            if (slot.Quantity == 0 || slot.ItemName == item.ItemName)
            {
                qty = slot.AddItem(item, qty);
                if (qty == 0)
                    return;
            }
        }

        PopupHelpers.ShowInventoryFullPopup(item.ItemName, qty);
    }

    /*
     * Removes quantity from a given slot.
     * Clears the slot if quantity reaches zero or below.
     */
    public void RemoveItem(ItemSlot slot, int qty)
    {
        if (slot == null)
            return;

        int newQty = slot.Quantity - qty;

        if (newQty <= 0)
        {
            slot.ClearSlot();
        }
        else
        {
            slot.AddItem(slot.ItemSO, -qty);
        }
    }

    /*
     * Removes an item by name from the first slot that contains it.
     */
    public void RemoveItemByName(string itemName, int qty)
    {
        foreach (var slot in allSlots)
        {
            if (slot.ItemSO != null && slot.ItemSO.ItemName == itemName)
            {
                RemoveItem(slot, qty);
                return;
            }
        }

        Debug.LogWarning("[InventorySlots] Tried to remove '" + itemName + "' but no slot contains it.");
    }

    /*
     * Returns the slot list corresponding to the item's category.
     */
    private List<ItemSlot> GetCategory(BaseItemSO item)
    {
        if (item is DiceSO)
            return diceSlots;

        if (item is PermanentSO)
            return permanentSlots;

        if (item is ConsumableSO || item is LootBoxSO)
            return consumableSlots;

        // Fallback
        return diceSlots;
    }

    /*
     * Handles click behavior for a slot when NOT in SellMode or ReplaceMode.
     * Dice / Permanent: select only.
     * LootBox: select on first click, open on second click.
     * Consumable: select on first click, use on second click.
     */
    public void HandleSlotClick(ItemSlot slot)
    {
        BaseItemSO item = slot.ItemSO;

        // Dice and permanent items: only selection
        if (item is DiceSO || item is PermanentSO)
        {
            slot.SelectSlot();
            return;
        }

        // LootBox: first click selects, second click opens and consumes
        if (item is LootBoxSO box)
        {
            if (!slot.ThisItemSelected)
            {
                slot.SelectSlot();
                return;
            }

            box.UseItem();
            InventoryManager.Instance.RemoveItem(slot, 1);
            return;
        }

        // Consumable: first click selects, second click uses and consumes
        if (item is ConsumableSO cons)
        {
            if (!slot.ThisItemSelected)
            {
                slot.SelectSlot();
                return;
            }

            cons.UseItem();
            InventoryManager.Instance.RemoveItem(slot, 1);
            return;
        }
    }

    /*
     * Swaps the contents of two slots (item + quantity).
     * Refreshes UI for both.
     */
    public void SwapSlots(ItemSlot a, ItemSlot b)
    {
        BaseItemSO soA = a.ItemSO;
        int qtyA = a.Quantity;

        BaseItemSO soB = b.ItemSO;
        int qtyB = b.Quantity;

        a.ClearSlot();
        b.ClearSlot();

        if (soB != null)
            a.AddItem(soB, qtyB);

        if (soA != null)
            b.AddItem(soA, qtyA);

        a.RefreshUI();
        b.RefreshUI();
    }

    // Replace mode state
    private BaseItemSO pendingItem;
    private int pendingQuantity;
    private bool waitingForReplace = false;

    public bool IsWaitingForReplace => waitingForReplace;

    /*
     * Enters replace mode with a pending item and quantity.
     * The next click on a target slot will apply the replacement.
     */
    public void PrepareReplace(BaseItemSO item, int quantity)
    {
        waitingForReplace = true;
        pendingItem = item;
        pendingQuantity = quantity;
    }

    /*
     * Applies the pending replacement to the target slot.
     * If the pending item is a consumable, it is applied to the target slot
     * via InventoryManager.PlaceConsumableOnSlot.
     * Otherwise, the target slot is overwritten with the pending item.
     */
    public void ReplaceInSlot(ItemSlot targetSlot)
    {
        if (!waitingForReplace)
            return;

        // Consumable replace: apply effect to target slot
        if (pendingItem is ConsumableSO)
        {
            ItemSlot consumableSlot = GetSlotHoldingPendingItem();
            InventoryManager.Instance.PlaceConsumableOnSlot(consumableSlot, targetSlot);

            waitingForReplace = false;
            pendingItem = null;
            pendingQuantity = 0;

            return;
        }

        // Normal replace: overwrite target slot with pending item
        targetSlot.ClearSlot();
        targetSlot.AddItem(pendingItem, pendingQuantity);

        waitingForReplace = false;
        pendingItem = null;
        pendingQuantity = 0;

        targetSlot.RefreshUI();

        // If this slot is an active dice slot, sync with ActiveDice system
        bool isActiveSlot = InventoryManager.Instance.ActiveDice.Contains(targetSlot);

        if (isActiveSlot)
            InventoryManager.Instance.ActiveDice.SyncSlot(targetSlot);
    }

    /*
     * Deselects all slots in the inventory.
     */
    public void DeselectAll()
    {
        foreach (var slot in allSlots)
            slot.DeselectSlot();
    }

    /*
     * Returns the slot that currently holds the pending item
     * used in replace mode (for consumables).
     */
    private ItemSlot GetSlotHoldingPendingItem()
    {
        foreach (var slot in allSlots)
        {
            if (slot.ItemSO == pendingItem)
                return slot;
        }
        return null;
    }
}
