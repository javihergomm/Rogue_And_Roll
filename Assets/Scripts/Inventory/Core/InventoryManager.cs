using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * InventoryManager
 * ----------------
 * Manages the inventory system:
 *  - Holds and manages item slots and active dice slots
 *  - Adds, removes and swaps items
 *  - Controls inventory UI visibility and soft-hide during drags
 *  - Applies permanent effects when items are added/removed
 *  - Places consumable items on Spots and ColorSpots (3D)
 */
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Slots")]
    [SerializeField] private InventorySlots slots;

    [Header("Active Dice")]
    [SerializeField] private ActiveDiceSlots activeDice;

    [Header("Permanent Effects")]
    [SerializeField] private InventoryPermanentEffects permanentEffects;

    [Header("Sell Mode")]
    [SerializeField] private InventorySellMode sellMode;

    [Header("UI")]
    [SerializeField] private GameObject inventoryMenu;

    public IReadOnlyList<ItemSlot> AllSlots => slots.AllSlots;
    public IReadOnlyList<ItemSlot> ItemSlots => slots.AllSlots;
    public ActiveDiceSlots ActiveDice => activeDice;

    public event Action OnInventoryChanged;
    public event Action OnActiveDiceChanged;

    private bool menuOpen = false;
    public bool IsOpen => menuOpen;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        Debug.Log("InventoryManager Awake");

        if (Instance != null && Instance != this)
        {
            Debug.Log("Duplicate InventoryManager destroyed");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        slots.Initialize();
        activeDice.Initialize(slots.ActiveDiceSlots);

        if (inventoryMenu != null)
        {
            canvasGroup = inventoryMenu.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = inventoryMenu.AddComponent<CanvasGroup>();
        }
    }
    public void AddStartingDice(DiceSO dice)
    {
        ItemSlot slot = activeDice.GetFirstEmptySlot();
        if (slot == null)
        {
            Debug.Log("No empty active dice slot for starting dice");
            return;
        }

        slot.AddItem(dice.ItemName, 1, dice.Icon, dice.Description);
        activeDice.SyncSlot(slot);
        OnActiveDiceChanged?.Invoke();
    }

    public BaseItemSO GetItemSO(string name)
    {
        Debug.Log("GetItemSO: " + name);
        return slots.GetItemSO(name);
    }

    public void AddItem(BaseItemSO item, int qty)
    {
        slots.AddItem(item, qty);
        permanentEffects.TryActivate(item);

        // Auto-use consumables
        if (item is ConsumableSO consumable && consumable.AutoUseOnPickup)
        {
            var ctx = new ConsumableContext();
            consumable.UseItem(ctx);

            if (ctx.WasUsed)
            {
                // Remove the item from inventory
                slots.RemoveItemByName(consumable.ItemName, 1);

            }
        }

        OnInventoryChanged?.Invoke();
    }



    public void RemoveItem(ItemSlot slot, int qty)
    {
        Debug.Log("RemoveItem: " + slot.ItemName + " x" + qty);

        BaseItemSO item = slots.GetItemSO(slot.ItemName);

        slots.RemoveItem(slot, qty);
        permanentEffects.TryDeactivate(item);

        if (activeDice.Contains(slot))
            activeDice.SyncSlot(slot);

        OnInventoryChanged?.Invoke();
    }

    public void HandleSlotClick(ItemSlot slot)
    {
        Debug.Log("HandleSlotClick: " + slot.ItemName);

        if (sellMode.IsActive)
        {
            sellMode.HandleClick(slot);
            return;
        }

        if (slots.IsWaitingForReplace)
        {
            slots.ReplaceInSlot(slot);
            CloseInventory();
            return;
        }

        slots.HandleSlotClick(slot);
    }

    public void HandleSlotDrop(ItemSlot from, ItemSlot to)
    {
        Debug.Log("HandleSlotDrop: " + from.ItemName + " -> " + to.ItemName);

        if (from == null || to == null)
            return;

        // Prevent swapping dice that have already been rolled this turn
        if (DiceRollManager.Instance.HasSlotRolledThisTurn(from))
        {
            Debug.Log("Cannot move a dice that has already been rolled this turn");
            return;
        }

        if (DiceRollManager.Instance.HasSlotRolledThisTurn(to))
        {
            Debug.Log("Cannot replace a dice that has already been rolled this turn");
            return;
        }

        BaseItemSO item = GetItemSO(from.ItemName);

        if (activeDice.Contains(to) && item is not DiceSO)
        {
            Debug.Log("Cannot drop non-dice into active dice slot");
            return;
        }

        slots.SwapSlots(from, to);

        activeDice.SyncSlot(from);
        activeDice.SyncSlot(to);

        OnActiveDiceChanged?.Invoke();
    }

    public int GetFinalDiceNumber()
    {
        return DiceRollManager.Instance.GetTotalRoll();
    }

    public void DeselectAllSlots()
    {
        Debug.Log("DeselectAllSlots");
        slots.DeselectAll();
    }

    public int GetActiveDiceSlotIndex(ItemSlot slot)
    {
        return activeDice.GetIndexOf(slot);
    }

    public void SetActiveSellPedestal(SellPedestal pedestal)
    {
        Debug.Log("SetActiveSellPedestal");
        sellMode.Enable(pedestal);
    }

    public void ClearActiveSellPedestal()
    {
        Debug.Log("ClearActiveSellPedestal");
        sellMode.Disable();
    }

    public void TryRemoveActiveDice(ItemSlot slot)
    {
        Debug.Log("TryRemoveActiveDice");

        if (activeDice.Contains(slot))
        {
            DiceRollManager.Instance.RemoveDiceFromWorld(slot);
            OnActiveDiceChanged?.Invoke();
        }
    }

    public void RefreshActiveDiceUI()
    {
        Debug.Log("RefreshActiveDiceUI");
        OnActiveDiceChanged?.Invoke();
    }

    public void PrepareReplace(BaseItemSO item, int quantity)
    {
        Debug.Log("PrepareReplace: " + item.ItemName);
        slots.PrepareReplace(item, quantity);
        OpenInventory();
    }

    public void ToggleInventory()
    {
        Debug.Log("ToggleInventory");

        if (CharacterSelectManager.Instance != null &&
            CharacterSelectManager.Instance.IsAnySelectorUIOpen())
            return;

        if (menuOpen)
            CloseInventory();
        else
            OpenInventory();
    }

    public void OpenInventory()
    {
        Debug.Log("OpenInventory");

        if (CharacterSelectManager.Instance != null &&
            CharacterSelectManager.Instance.IsAnySelectorUIOpen())
            return;

        if (menuOpen)
            return;

        menuOpen = true;

        if (inventoryMenu != null)
            inventoryMenu.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }

        foreach (var slot in slots.AllSlots)
            slot.RefreshUI();

        Time.timeScale = 0f;
    }

    public void CloseInventory()
    {
        Debug.Log("CloseInventory");

        if (!menuOpen)
            return;

        menuOpen = false;

        if (inventoryMenu != null)
            inventoryMenu.SetActive(false);

        slots.DeselectAll();
        sellMode.Disable();

        Time.timeScale = 1f;
    }

    public void HideInventorySoft()
    {
        Debug.Log("HideInventorySoft");

        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void PlaceConsumableOnSpot(ItemSlot slot, Spot spot)
    {
        Debug.Log("PlaceConsumableOnSpot: " + slot.ItemName);
        PlaceConsumableInternal(slot, spot);
    }

    public void PlaceConsumableOnColorSpot(ItemSlot slot, ColorSpot colorSpot)
    {
        Debug.Log("PlaceConsumableOnColorSpot: " + slot.ItemName);
        PlaceConsumableInternal(slot, colorSpot);
    }

    private void PlaceConsumableInternal(ItemSlot slot, MonoBehaviour target)
    {
        Debug.Log("PlaceConsumableInternal target=" + target);

        if (slot == null || target == null)
        {
            Debug.Log("Null slot or target");
            return;
        }

        BaseItemSO item = GetItemSO(slot.ItemName);
        if (item is not ConsumableSO consumable)
        {
            Debug.Log("Item is not ConsumableSO");
            return;
        }

        Debug.Log("Consumable detected: " + consumable.ItemName);

        ConsumableContext ctx = new();

        if (target is ColorSpot colorSpot)
        {
            Debug.Log("Target is ColorSpot");
            ctx.TargetColorSpot = colorSpot;
        }
        else if (target is Spot spot)
        {
            Debug.Log("Target is Spot");
            ctx.TargetSpot = spot;
        }
        else
        {
            Debug.Log("Unsupported target type");
            return;
        }

        Debug.Log("Calling UseItem...");
        consumable.UseItem(ctx);
        Debug.Log("UseItem finished. WasUsed=" + ctx.WasUsed);

        if (!ctx.WasUsed)
        {
            Debug.Log("Consumable was NOT used. Aborting.");
            return;
        }

        Debug.Log("ColorSpot target confirmed (no prefab instantiation)");

        Debug.Log("Removing item from inventory");
        RemoveItem(slot, 1);
    }
}