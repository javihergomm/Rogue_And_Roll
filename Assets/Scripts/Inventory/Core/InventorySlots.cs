using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/*
 * InventorySlots
 * --------------
 * Handles all inventory slot groups and item operations.
 * Manages adding, removing, swapping, selecting, and replacing items.
 * Provides access to item data and organizes items by category.
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
     * Initializes the slot system and builds the item lookup table.
     */
    public void Initialize()
    {
        lookup = new Dictionary<string, BaseItemSO>();
        foreach (var item in itemSOs)
            lookup[item.ItemName] = item;

        allSlots.Clear();
        allSlots.AddRange(activeDiceSlots);
        allSlots.AddRange(diceSlots);
        allSlots.AddRange(permanentSlots);
        allSlots.AddRange(consumableSlots);
    }

    /*
     * Returns the ScriptableObject for an item by name.
     * (Now delegated to InventoryManager’s catalog)
     */
    public BaseItemSO GetItemSO(string name)
    {
        return InventoryManager.Instance.GetItemSO(name);
    }

    /*
     * Adds an item to the correct category of slots.
     * Fills existing stacks first, then empty slots.
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
     * Removes a quantity of an item from a slot.
     * Clears the slot if quantity reaches zero.
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
     * Removes an item by name (used for auto-use consumables)
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
     * Returns the correct slot group for an item type.
     */
    private List<ItemSlot> GetCategory(BaseItemSO item)
    {
        if (item is DiceSO)
            return diceSlots;

        if (item is PermanentSO)
            return permanentSlots;

        if (item is ConsumableSO || item is LootBoxSO)
            return consumableSlots;

        return diceSlots;
    }

    /*
     * Handles clicking a slot depending on the item type.
     */
    public void HandleSlotClick(ItemSlot slot)
    {
        BaseItemSO item = slot.ItemSO;

        if (item is DiceSO || item is PermanentSO)
        {
            slot.SelectSlot();
            return;
        }

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
     * Swaps the contents of two slots.
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

    private BaseItemSO pendingItem;
    private int pendingQuantity;
    private bool waitingForReplace = false;

    public bool IsWaitingForReplace => waitingForReplace;

    /*
     * Stores data for replacing an item in a slot.
     */
    public void PrepareReplace(BaseItemSO item, int quantity)
    {
        waitingForReplace = true;
        pendingItem = item;
        pendingQuantity = quantity;
    }

    /*
     * Replaces the contents of a slot with the pending item.
     */
    public void ReplaceInSlot(ItemSlot slot)
    {
        if (!waitingForReplace)
            return;

        slot.ClearSlot();
        slot.AddItem(pendingItem, pendingQuantity);

        waitingForReplace = false;
        pendingItem = null;
        pendingQuantity = 0;

        slot.RefreshUI();

        bool isActiveSlot = InventoryManager.Instance.ActiveDice.Contains(slot);

        if (isActiveSlot)
            InventoryManager.Instance.ActiveDice.SyncSlot(slot);
    }

    /*
     * Deselects all slots in the inventory.
     */
    public void DeselectAll()
    {
        foreach (var slot in allSlots)
            slot.DeselectSlot();
    }
}
