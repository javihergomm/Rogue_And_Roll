using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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

    public void Initialize()
    {
        lookup = new Dictionary<string, BaseItemSO>();
        foreach (var item in itemSOs)
            lookup[item.ItemName] = item;

        allSlots.Clear();

        // Assign slot types
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

    public BaseItemSO GetItemSO(string name)
    {
        return InventoryManager.Instance.GetItemSO(name);
    }

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

    public void PrepareReplace(BaseItemSO item, int quantity)
    {
        waitingForReplace = true;
        pendingItem = item;
        pendingQuantity = quantity;
    }

    public void ReplaceInSlot(ItemSlot targetSlot)
    {
        if (!waitingForReplace)
            return;

        if (pendingItem is ConsumableSO)
        {
            ItemSlot consumableSlot = GetSlotHoldingPendingItem();
            InventoryManager.Instance.PlaceConsumableOnSlot(consumableSlot, targetSlot);

            waitingForReplace = false;
            pendingItem = null;
            pendingQuantity = 0;

            return;
        }

        targetSlot.ClearSlot();
        targetSlot.AddItem(pendingItem, pendingQuantity);

        waitingForReplace = false;
        pendingItem = null;
        pendingQuantity = 0;

        targetSlot.RefreshUI();

        bool isActiveSlot = InventoryManager.Instance.ActiveDice.Contains(targetSlot);

        if (isActiveSlot)
            InventoryManager.Instance.ActiveDice.SyncSlot(targetSlot);
    }

    public void DeselectAll()
    {
        foreach (var slot in allSlots)
            slot.DeselectSlot();
    }

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
