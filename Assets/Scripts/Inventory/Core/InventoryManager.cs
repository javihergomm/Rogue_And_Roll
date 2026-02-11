using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * InventoryManager
 * ----------------
 * Controls the inventory system:
 *  - Manages item slots and active dice slots
 *  - Handles adding, removing, swapping and selecting items
 *  - Controls inventory UI visibility
 *  - Supports soft-hide mode for drag operations
 *  - Places consumables on board spots
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

    [Header("Starting Dice")]
    [SerializeField] private DiceSO startingDice;

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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        slots.Initialize();
        activeDice.Initialize(slots.ActiveDiceSlots);

        canvasGroup = inventoryMenu.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = inventoryMenu.AddComponent<CanvasGroup>();
    }

    private void Start()
    {
        GiveStartingDice();
        OnActiveDiceChanged?.Invoke();
    }

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

    public BaseItemSO GetItemSO(string name)
    {
        return slots.GetItemSO(name);
    }

    public void AddItem(BaseItemSO item, int qty)
    {
        slots.AddItem(item, qty);
        permanentEffects.TryActivate(item);
        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(ItemSlot slot, int qty)
    {
        BaseItemSO item = slots.GetItemSO(slot.ItemName);

        slots.RemoveItem(slot, qty);
        permanentEffects.TryDeactivate(item);

        if (activeDice.Contains(slot))
            activeDice.SyncSlot(slot);

        OnInventoryChanged?.Invoke();
    }

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
     * Handles drag-and-drop between two slots.
     */
    public void HandleSlotDrop(ItemSlot from, ItemSlot to)
    {
        if (from == null || to == null)
            return;

        BaseItemSO item = GetItemSO(from.ItemName);

        if (activeDice.Contains(to) && !(item is DiceSO))
            return;

        slots.SwapSlots(from, to);

        activeDice.SyncSlot(from);
        activeDice.SyncSlot(to);

        OnActiveDiceChanged?.Invoke();
    }

    public int GetFinalDiceNumber()
    {
        ItemSlot slot = activeDice.GetSelectedSlot();
        if (slot == null)
            return 0;

        var info = DiceRollManager.Instance.GetRollInfo(slot);
        return info?.finalRoll ?? 0;
    }

    public void DeselectAllSlots()
    {
        slots.DeselectAll();
    }

    public int GetActiveDiceSlotIndex(ItemSlot slot)
    {
        return activeDice.GetIndexOf(slot);
    }

    public void SetActiveSellPedestal(SellPedestal pedestal)
    {
        sellMode.Enable(pedestal);
    }

    public void ClearActiveSellPedestal()
    {
        sellMode.Disable();
    }

    public void TryRemoveActiveDice(ItemSlot slot)
    {
        if (activeDice.Contains(slot))
        {
            DiceRollManager.Instance.RemoveDiceFromWorld(slot);
            OnActiveDiceChanged?.Invoke();
        }
    }

    public void RefreshActiveDiceUI()
    {
        OnActiveDiceChanged?.Invoke();
    }

    public void PrepareReplace(BaseItemSO item, int quantity)
    {
        slots.PrepareReplace(item, quantity);
        OpenInventory();
    }

    /*
     * Toggles the inventory using real open/close.
     */
    public void ToggleInventory()
    {
        if (menuOpen)
            CloseInventory();
        else
            OpenInventory();
    }

    /*
     * Opens the inventory using real activation.
     */
    public void OpenInventory()
    {
        if (menuOpen)
            return;

        menuOpen = true;

        inventoryMenu.SetActive(true);

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        foreach (var slot in slots.AllSlots)
            slot.RefreshUI();

        Time.timeScale = 0f;
    }

    /*
     * Closes the inventory using real deactivation.
     */
    public void CloseInventory()
    {
        if (!menuOpen)
            return;

        menuOpen = false;

        inventoryMenu.SetActive(false);

        slots.DeselectAll();
        sellMode.Disable();

        Time.timeScale = 1f;
    }

    /*
     * Soft-hides the inventory during drag operations.
     * The menu becomes invisible but stays active.
     */
    public void HideInventorySoft()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    /*
     * Places a consumable on a board spot.
     */
    public void PlaceConsumableOnSpot(ItemSlot slot, Spot spot)
    {
        BaseItemSO item = GetItemSO(slot.ItemName);
        if (!(item is ConsumableSO))
            return;

        if (item.Prefab3D != null)
            Instantiate(item.Prefab3D, spot.transform.position, Quaternion.identity);

        RemoveItem(slot, 1);
    }
}
