using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * InventoryManager
 * ----------------
 * Central controller for the player's inventory.
 * Handles:
 *   - Item storage (slots)
 *   - Active dice slots
 *   - Permanent effects
 *   - Sell mode
 *   - Inventory UI
 *   - LootBox reward handling
 *   - Consumable placement
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
    [SerializeField] private InventoryDescriptionUI descriptionUI;
    public InventoryDescriptionUI DescriptionUI => descriptionUI;

    public IReadOnlyList<ItemSlot> AllSlots => slots.AllSlots;
    public ActiveDiceSlots ActiveDice => activeDice;

    public event Action OnInventoryChanged;
    public event Action OnActiveDiceChanged;

    private bool menuOpen = false;
    public bool IsOpen => menuOpen;

    private CanvasGroup canvasGroup;

    // Catalog of all items loaded from Resources
    private Dictionary<string, BaseItemSO> itemCatalog = new();

    [SerializeField] private Transform playerDiceArea;
    public Transform PlayerDiceArea => playerDiceArea;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Load all items into catalog
        LoadItemCatalog();

        // Load unlock states
        Unlocks.Load();

        // Initialize slot systems
        slots.Initialize();
        activeDice.Initialize(slots.ActiveDiceSlots);

        // Prepare UI canvas group
        if (inventoryMenu != null)
        {
            canvasGroup = inventoryMenu.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = inventoryMenu.AddComponent<CanvasGroup>();
        }

        // Listen to lootbox events
        LootBoxEvents.OnLootBoxOpened += HandleLootBoxReward;
    }
    private void Start()
    {
        // ... tu código existente de carga de catálogo, slots, etc.

        // ============================================================
        // GIVE 1 LIMO AT GAME START
        // ============================================================

        if (itemCatalog.TryGetValue("item_special_slime_even_only", out BaseItemSO limo))
        {
            AddItem(limo, 1);
        }
        else
        {
            Debug.LogError("InventoryManager: item_consumable_limo not found in catalog.");
        }
    }


    /*
     * Loads all item ScriptableObjects from Resources/Items folders
     * into a dictionary for fast lookup by itemID.
     */
    private void LoadItemCatalog()
    {
        itemCatalog.Clear();

        string[] folders = { "Dice", "Consumables", "Permanents", "LootBox" };

        foreach (var folder in folders)
        {
            var items = Resources.LoadAll<BaseItemSO>("Items/" + folder);
            foreach (var item in items)
            {
                if (item != null)
                    itemCatalog[item.itemID] = item;
            }
        }
    }

    /*
     * Returns the ScriptableObject for a given item ID.
     */
    public BaseItemSO GetItemSO(string id)
    {
        if (itemCatalog.TryGetValue(id, out var item))
            return item;

        return null;
    }

    /*
     * Adds an item to the inventory.
     * Also activates permanent effects and auto-uses consumables if needed.
     */
    public void AddItem(BaseItemSO item, int qty)
    {
        slots.AddItem(item, qty);

        // Activate permanent effects if applicable
        permanentEffects.TryActivate(item);

        // Auto-use consumables on pickup
        if (item is ConsumableSO cons && cons.AutoUseOnPickup)
        {
            var ctx = new ConsumableContext();
            cons.UseItem(ctx);

            if (ctx.WasUsed)
                slots.RemoveItemByName(cons.ItemName, 1);
        }

        OnInventoryChanged?.Invoke();
    }

    /*
     * Removes an item from a slot.
     * Handles permanent effect deactivation and active dice sync.
     */
    public void RemoveItem(ItemSlot slot, int qty)
    {
        BaseItemSO item = slot.ItemSO;

        // Permanent items that cannot be removed
        if (item is PermanentSO perm && perm.CannotBeUnequipped)
            return;

        slots.RemoveItem(slot, qty);
        permanentEffects.TryDeactivate(item);

        // If the removed item was an active dice, sync world dice
        if (activeDice.Contains(slot))
            activeDice.SyncSlot(slot);

        OnInventoryChanged?.Invoke();
    }

    /*
     * Handles clicking on an inventory slot.
     * Supports:
     *   - Sell mode
     *   - Replace mode
     *   - Normal selection
     */
    public void HandleSlotClick(ItemSlot slot)
    {
        if (slot == null)
            return;

        BaseItemSO item = slot.ItemSO;

        // Permanent items cannot be unequipped
        if (item is PermanentSO perm && perm.CannotBeUnequipped)
            return;

        // Sell mode
        if (sellMode.IsActive)
        {
            sellMode.HandleClick(slot);
            return;
        }

        // Replace mode
        if (slots.IsWaitingForReplace)
        {
            slots.ReplaceInSlot(slot);
            CloseInventory();
            return;
        }

        // Normal click
        slots.HandleSlotClick(slot);
    }

    /*
     * Handles dragging/dropping items between slots.
     * Prevents placing non-dice items into active dice slots.
     */
    public void HandleSlotDrop(ItemSlot from, ItemSlot to)
    {
        if (from == null || to == null)
            return;

        BaseItemSO item = from.ItemSO;

        // Only dice can be placed in active dice slots
        if (activeDice.Contains(to) && item is not DiceSO)
            return;

        slots.SwapSlots(from, to);

        activeDice.SyncSlot(from);
        activeDice.SyncSlot(to);

        OnActiveDiceChanged?.Invoke();
    }

    // ---------------------------------------------------------
    // ACTIVE DICE API
    // ---------------------------------------------------------

    public int GetFinalDiceNumber()
    {
        return DiceRollManager.Instance.GetTotalRoll();
    }

    public int GetActiveDiceSlotIndex(ItemSlot slot)
    {
        return activeDice.GetIndexOf(slot);
    }

    public void AddStartingDice(DiceSO dice)
    {
        ItemSlot slot = activeDice.GetFirstEmptySlot();
        if (slot == null)
            return;

        slot.AddItem(dice, 1);
        activeDice.SyncSlot(slot);
        OnActiveDiceChanged?.Invoke();
    }

    public void TryRemoveActiveDice(ItemSlot slot)
    {
        if (activeDice.Contains(slot))
        {
            DiceRollManager.Instance.RemoveDiceFromWorld(slot);
            OnActiveDiceChanged?.Invoke();
        }
    }

    public void DeselectAllSlots()
    {
        slots.DeselectAll();
    }

    public void RefreshActiveDiceUI()
    {
        OnActiveDiceChanged?.Invoke();
    }

    // ---------------------------------------------------------
    // SELL MODE
    // ---------------------------------------------------------

    public void SetActiveSellPedestal(SellPedestal pedestal)
    {
        sellMode.Enable(pedestal);
    }

    public void ClearActiveSellPedestal()
    {
        sellMode.Disable();
    }

    // ---------------------------------------------------------
    // REPLACE MODE
    // ---------------------------------------------------------

    public void PrepareReplace(BaseItemSO item, int quantity)
    {
        if (item == null)
            return;

        if (item is PermanentSO perm && perm.CannotBeUnequipped)
            return;

        slots.PrepareReplace(item, quantity);
        OpenInventory();
    }

    // ---------------------------------------------------------
    // INVENTORY UI
    // ---------------------------------------------------------

    public void ToggleInventory()
    {
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
        if (canvasGroup == null)
            return;

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    // ---------------------------------------------------------
    // CONSUMABLES
    // ---------------------------------------------------------

    public void PlaceConsumableOnSpot(ItemSlot slot, Spot spot)
    {
        PlaceConsumableInternal(slot, spot);
    }

    public void PlaceConsumableOnColorSpot(ItemSlot slot, ColorSpot colorSpot)
    {
        PlaceConsumableInternal(slot, colorSpot);
    }

    /*
     * Internal helper for placing consumables on spots.
     * Builds a ConsumableContext and executes the effect.
     */
    private void PlaceConsumableInternal(ItemSlot slot, MonoBehaviour target)
    {
        if (slot == null || target == null)
            return;

        BaseItemSO item = slot.ItemSO;
        if (item is not ConsumableSO cons)
            return;

        ConsumableContext ctx = new();

        if (target is ColorSpot cs)
            ctx.TargetColorSpot = cs;
        else if (target is Spot s)
            ctx.TargetSpot = s;
        else
            return;

        cons.UseItem(ctx);

        if (ctx.WasUsed)
            RemoveItem(slot, 1);
    }

    public void PlaceConsumableOnSlot(ItemSlot consumableSlot, ItemSlot targetSlot)
    {
        if (consumableSlot == null || targetSlot == null)
            return;

        BaseItemSO item = consumableSlot.ItemSO;
        if (item is not ConsumableSO cons)
            return;

        ConsumableContext ctx = new();
        ctx.TargetSlot = targetSlot;

        cons.UseItem(ctx);

        if (ctx.WasUsed)
            RemoveItem(consumableSlot, 1);
    }

    // ---------------------------------------------------------
    // LOOTBOX HANDLING
    // ---------------------------------------------------------

    /*
     * Called when a lootbox is opened.
     * Adds the reward item to the inventory.
     */
    private void HandleLootBoxReward(LootBoxSO box, BaseItemSO reward)
    {
        AddItem(reward, 1);
    }

    // ---------------------------------------------------------
    // DICE ROLL CHECK
    // ---------------------------------------------------------

    /*
     * Returns true if all active dice have finished rolling.
     * Used by DiceRollManager to know when movement can start.
     */
    public bool AllDiceFinishedRolling()
    {
        foreach (ItemSlot slot in ActiveDice.GetNonEmptySlots())
        {
            var info = DiceRollManager.Instance.GetRollInfo(slot);
            if (!info.HasValue)
                return false;
        }

        return true;
    }
}
