using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * InventoryManager
 * ----------------
 * Main controller for the game's inventory system.
 * Handles item storage, active dice, permanent effects,
 * selling mode, UI visibility, and item interactions.
 */

public class InventoryManager : MonoBehaviour
{
    // Singleton instance so other scripts can access the inventory easily
    public static InventoryManager Instance { get; private set; }

    // Reference to all inventory slots (normal item slots)
    [Header("Slots")]
    [SerializeField] private InventorySlots slots;

    // Reference to the slots that hold active dice
    [Header("Active Dice")]
    [SerializeField] private ActiveDiceSlots activeDice;

    // Handles passive effects that stay active while the item is owned
    [Header("Permanent Effects")]
    [SerializeField] private InventoryPermanentEffects permanentEffects;

    // Handles selling mode logic
    [Header("Sell Mode")]
    [SerializeField] private InventorySellMode sellMode;

    // UI panel for the inventory menu
    [Header("UI")]
    [SerializeField] private GameObject inventoryMenu;

    // Dice given to the player at the start of the game
    [Header("Starting Dice")]
    [SerializeField] private DiceSO startingDice;

    // Public access to all item slots
    public IReadOnlyList<ItemSlot> AllSlots => slots.AllSlots;
    public IReadOnlyList<ItemSlot> ItemSlots => slots.AllSlots;

    // Public access to active dice slots
    public ActiveDiceSlots ActiveDice => activeDice;

    // Events fired when inventory or active dice change
    public event Action OnInventoryChanged;
    public event Action OnActiveDiceChanged;

    // Tracks whether the inventory menu is currently open
    private bool menuOpen = false;

    // Public read-only property to check if the inventory is open
    public bool IsOpen => menuOpen;

    private void Awake()
    {
        // Ensure only one InventoryManager exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Initialize all slot systems
        slots.Initialize();
        activeDice.Initialize(slots.ActiveDiceSlots);
    }

    private void Start()
    {
        // Give the player their starting dice
        GiveStartingDice();

        // Notify UI that active dice changed
        OnActiveDiceChanged?.Invoke();
    }

    /*
     * Gives the player their initial dice at the start of the game.
     * Places the dice in the first available active dice slot.
     */
    private void GiveStartingDice()
    {
        if (startingDice == null)
            return;

        ItemSlot slot = activeDice.GetFirstEmptySlot();
        if (slot == null)
            return;

        slot.AddItem(
            startingDice.ItemName,
            1,
            startingDice.Icon,
            startingDice.Description
        );

        activeDice.SyncSlot(slot);
        OnActiveDiceChanged?.Invoke();
    }

    /*
     * Returns the ScriptableObject data for an item by name.
     * Used to retrieve icons, prefabs, descriptions, etc.
     */
    public BaseItemSO GetItemSO(string name)
    {
        return slots.GetItemSO(name);
    }

    /*
     * Adds an item to the inventory.
     * Also activates any permanent effects the item may have.
     */
    public void AddItem(BaseItemSO item, int qty)
    {
        slots.AddItem(item, qty);
        permanentEffects.TryActivate(item);
        OnInventoryChanged?.Invoke();
    }

    /*
     * Removes an item from a specific slot.
     * Also deactivates permanent effects if needed.
     */
    public void RemoveItem(ItemSlot slot, int qty)
    {
        BaseItemSO item = slots.GetItemSO(slot.ItemName);

        slots.RemoveItem(slot, qty);
        permanentEffects.TryDeactivate(item);

        // If the removed item was an active dice, update the dice UI
        if (activeDice.Contains(slot))
            activeDice.SyncSlot(slot);

        OnInventoryChanged?.Invoke();
    }

    /*
     * Handles left-click interactions on a slot.
     * This includes selecting items, replacing items, and selling items.
     */
    public void HandleSlotClick(ItemSlot slot)
    {
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

    /*
     * Handles drag-and-drop between two inventory slots.
     * Used for rearranging items and dice inside the inventory.
     */
    public void HandleSlotDrop(ItemSlot from, ItemSlot to)
    {
        slots.SwapSlots(from, to);
        activeDice.SyncSlot(from);
        activeDice.SyncSlot(to);
        OnActiveDiceChanged?.Invoke();
    }

    /*
     * Returns the final dice roll result for the currently selected active dice.
     */
    public int GetFinalDiceNumber()
    {
        ItemSlot slot = activeDice.GetSelectedSlot();
        if (slot == null)
            return 0;

        var info = DiceRollManager.Instance.GetRollInfo(slot);
        return info?.finalRoll ?? 0;
    }

    /*
     * Deselects all inventory slots.
     */
    public void DeselectAllSlots()
    {
        slots.DeselectAll();
    }

    /*
     * Returns the index of an active dice slot.
     */
    public int GetActiveDiceSlotIndex(ItemSlot slot)
    {
        return activeDice.GetIndexOf(slot);
    }

    /*
     * Enables selling mode for a specific pedestal.
     */
    public void SetActiveSellPedestal(SellPedestal pedestal)
    {
        sellMode.Enable(pedestal);
    }

    /*
     * Disables selling mode.
     */
    public void ClearActiveSellPedestal()
    {
        sellMode.Disable();
    }

    /*
     * Removes a dice from the world if it is removed from the active dice slots.
     */
    public void TryRemoveActiveDice(ItemSlot slot)
    {
        if (activeDice.Contains(slot))
        {
            DiceRollManager.Instance.RemoveDiceFromWorld(slot);
            OnActiveDiceChanged?.Invoke();
        }
    }

    /*
     * Refreshes the UI for active dice.
     */
    public void RefreshActiveDiceUI()
    {
        OnActiveDiceChanged?.Invoke();
    }

    /*
     * Prepares the inventory to replace an item with another one.
     */
    public void PrepareReplace(BaseItemSO item, int quantity)
    {
        slots.PrepareReplace(item, quantity);
        OpenInventory();
    }

    /*
     * Toggles the inventory menu on or off.
     */
    public void ToggleInventory()
    {
        if (CharacterSelectManager.Instance != null &&
            CharacterSelectManager.Instance.IsSelectorOpen())
            return;

        if (menuOpen)
            CloseInventory();
        else
            OpenInventory();
    }

    /*
     * Opens the inventory menu and pauses the game.
     */
    public void OpenInventory()
    {
        if (CharacterSelectManager.Instance != null &&
            CharacterSelectManager.Instance.IsSelectorOpen())
            return;

        if (menuOpen)
            return;

        menuOpen = true;
        inventoryMenu?.SetActive(true);

        foreach (var slot in slots.AllSlots)
            slot.RefreshUI();

        Time.timeScale = 0f;
    }

    /*
     * Opens the inventory menu with optional pause control.
     */
    public void OpenInventory(bool pauseGame)
    {
        OpenInventory();
        Time.timeScale = pauseGame ? 0f : 1f;
    }

    /*
     * Closes the inventory menu and resumes the game.
     */
    public void CloseInventory()
    {
        if (!menuOpen)
            return;

        menuOpen = false;
        inventoryMenu?.SetActive(false);

        slots.DeselectAll();
        sellMode.Disable();

        Time.timeScale = 1f;
    }

    /*
     * Places a consumable item on a board spot when dragged from the inventory.
     * Instantiates the item's prefab at the spot's position.
     * Removes one unit of the item from the inventory.
     */
    public void PlaceConsumableOnSpot(ItemSlot slot, Spot spot)
    {
        BaseItemSO item = GetItemSO(slot.ItemName);
        if (!(item is ConsumableSO))
            return;

        if (item.Prefab3D != null)
        {
            Instantiate(item.Prefab3D, spot.transform.position, Quaternion.identity);
        }

        RemoveItem(slot, 1);

        // Reopen inventory after placing the consumable
        OpenInventory();
    }
}
