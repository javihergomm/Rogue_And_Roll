using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/*
 * InventoryManager
 * ----------------
 * Central system for managing the player's inventory.
 * Handles item selection, dice spawning, consumable usage,
 * replacement mode, active dice slots, and inventory UI.
 */
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Active Dice Slots (3 max)")]
    [SerializeField] private List<ItemSlot> activeDiceSlots;

    [Header("Inventory Slots")]
    [SerializeField] private List<ItemSlot> diceSlots;
    [SerializeField] private List<ItemSlot> permanentSlots;
    [SerializeField] private List<ItemSlot> consumableSlots;

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private TMP_Text activeDiceText;

    [Header("Item Data")]
    [SerializeField] private BaseItemSO[] itemSOs;

    private readonly List<ItemSlot> allSlots = new List<ItemSlot>();
    public IReadOnlyList<ItemSlot> ItemSlots => allSlots;

    private bool waitingForReplace = false;
    private BaseItemSO pendingItem;
    private int pendingQuantity;

    private bool menuActivated = false;
    private SellPedestal activeSellPedestal;

    private Dictionary<string, BaseItemSO> itemLookup;

    private ItemSlot activeDiceSlot;
    public ItemSlot ActiveDiceSlot => activeDiceSlot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        allSlots.Clear();
        allSlots.AddRange(activeDiceSlots);
        allSlots.AddRange(diceSlots);
        allSlots.AddRange(permanentSlots);
        allSlots.AddRange(consumableSlots);

        if (inventoryMenu != null)
            inventoryMenu.SetActive(false);

        itemLookup = new Dictionary<string, BaseItemSO>();
        foreach (var item in itemSOs)
            if (!itemLookup.ContainsKey(item.itemName))
                itemLookup[item.itemName] = item;

        if (activeDiceText != null)
            activeDiceText.text = "Ningun dado activo";
    }

    private void Start()
    {
        // DEBUG: print all slots and their contents
        Debug.Log("========== DEBUG INVENTARIO AL INICIAR ==========");
        foreach (var slot in allSlots)
        {
            Debug.Log(slot.name + " -> item=" + slot.itemName + " qty=" + slot.quantity);
        }
        Debug.Log("=================================================");

        DiceSO d6Item = itemSOs.OfType<DiceSO>().FirstOrDefault(d => d.itemName == "D6");
        if (d6Item == null) return;

        ItemSlot d6Slot = AddDiceToActiveSlots(d6Item);
        if (d6Slot == null) return;

        activeDiceSlot = d6Slot;
        UpdateActiveDiceUI();

        SyncActiveDiceSlot(d6Slot);
    }


    // -------------------------------------------------------------------------
    // Active dice slot assignment
    // -------------------------------------------------------------------------
    private ItemSlot AddDiceToActiveSlots(DiceSO dice)
    {
        foreach (var slot in activeDiceSlots)
        {
            if (slot.quantity == 0)
            {
                slot.AddItem(dice.itemName, 1, dice.icon, dice.itemDescription);
                SyncActiveDiceSlot(slot);
                return slot;
            }
        }
        return null;
    }

    public int GetActiveDiceSlotIndex(ItemSlot slot)
    {
        return activeDiceSlots.IndexOf(slot);
    }

    private void SyncActiveDiceSlot(ItemSlot slot)
    {
        if (slot.quantity == 0)
        {
            DiceRollManager.Instance.RemoveDiceFromWorld(slot);
            return;
        }

        BaseItemSO item = GetItemSO(slot.itemName);
        if (item is DiceSO dice)
            DiceRollManager.Instance.SpawnDiceInWorld(dice, slot);
    }

    public void SetActiveDice(ItemSlot targetSlot, DiceSO dice)
    {
        targetSlot.ClearSlot();
        targetSlot.AddItem(dice.itemName, 1, dice.icon, dice.itemDescription);
        SyncActiveDiceSlot(targetSlot);

        activeDiceSlot = targetSlot;
        UpdateActiveDiceUI();
    }

    // -------------------------------------------------------------------------
    // Item lookup
    // -------------------------------------------------------------------------
    public BaseItemSO GetItemSO(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        return itemLookup.TryGetValue(itemName, out var result) ? result : null;
    }

    // -------------------------------------------------------------------------
    // Slot click handling
    // -------------------------------------------------------------------------
    public void OnSlotClicked(ItemSlot slot)
    {
        // Prevent shop items from triggering inventory actions
        if (slot.transform.root.GetComponent<ShopExitManager>() != null)
        {
            Debug.Log("[DEBUG] Click blocked: this object belongs to the shop");
            return;
        }

        bool wasSelected = slot.thisItemSelected;

        if (waitingForReplace)
        {
            ReplaceInSlot(slot);
            return;
        }

        if (activeSellPedestal != null)
        {
            activeSellPedestal.OnItemClicked(slot);
            return;
        }

        BaseItemSO item = GetItemSO(slot.itemName);

        // DICE LOGIC (inventory click DOES NOT roll)
        if (item is DiceSO)
        {
            slot.SelectSlot();
            return;
        }

        // PERMANENT ITEMS
        if (item is PermanentSO)
        {
            slot.SelectSlot();
            return;
        }

        // CONSUMABLE ITEMS
        if (item is ConsumableSO cons)
        {
            if (!wasSelected)
            {
                slot.SelectSlot();
                return;
            }

            StatManager.Instance?.TryUseItem(cons, slot);
            return;
        }
    }

    public void HandleSlotDrop(ItemSlot from, ItemSlot to)
    {
        BaseItemSO item = GetItemSO(from.itemName);

        // ---------------------------------------------------------
        // BLOCK INVALID ITEMS FROM ENTERING ACTIVE DICE SLOTS
        // ---------------------------------------------------------
        if (activeDiceSlots.Contains(to))
        {
            // Slot is empty -> cannot be dropped into active dice slot
            if (string.IsNullOrEmpty(from.itemName) || from.quantity <= 0)
                return;

            // Item is not a dice -> cannot enter active dice slot
            if (!(item is DiceSO))
                return;
        }

        // ---------------------------------------------------------
        // DICE LOGIC
        // ---------------------------------------------------------
        if (item is DiceSO dice)
        {
            // Move into active slot
            if (activeDiceSlots.Contains(to))
            {
                to.ClearSlot();
                to.AddItem(dice.itemName, 1, dice.icon, dice.itemDescription);

                from.ClearSlot();

                SyncActiveDiceSlot(to);
                SyncActiveDiceSlot(from);

                activeDiceSlot = null;
                UpdateActiveDiceUI();
                return;
            }

            // Move out of active slot into normal dice slot
            if (activeDiceSlots.Contains(from) && diceSlots.Contains(to))
            {
                to.ClearSlot();
                to.AddItem(dice.itemName, 1, dice.icon, dice.itemDescription);

                from.ClearSlot();

                SyncActiveDiceSlot(from);
                SyncActiveDiceSlot(to);

                if (activeDiceSlot == from)
                    activeDiceSlot = null;

                UpdateActiveDiceUI();
                return;
            }
        }

        // ---------------------------------------------------------
        // DEFAULT: swap items normally
        // ---------------------------------------------------------
        SwapSlots(from, to);
    }


    private void SwapSlots(ItemSlot a, ItemSlot b)
    {
        string nameA = a.itemName;
        int qtyA = a.quantity;
        Sprite spriteA = a.itemSprite;
        string descA = a.itemDescription;

        a.itemName = b.itemName;
        a.quantity = b.quantity;
        a.itemSprite = b.itemSprite;
        a.itemDescription = b.itemDescription;

        b.itemName = nameA;
        b.quantity = qtyA;
        b.itemSprite = spriteA;
        b.itemDescription = descA;

        a.RefreshUI();
        b.RefreshUI();
    }

    // -------------------------------------------------------------------------
    // Replace mode
    // -------------------------------------------------------------------------
    public void PrepareReplace(BaseItemSO item, int quantity)
    {
        waitingForReplace = true;
        pendingItem = item;
        pendingQuantity = quantity;

        OpenInventory();
    }

    private void ReplaceInSlot(ItemSlot slot)
    {
        slot.ClearSlot();

        slot.AddItem(
            pendingItem.itemName,
            pendingQuantity,
            pendingItem.icon,
            pendingItem.itemDescription
        );

        waitingForReplace = false;
        pendingItem = null;
        pendingQuantity = 0;

        SyncActiveDiceSlot(slot);
        CloseInventory();
    }

    // -------------------------------------------------------------------------
    // Sell pedestal
    // -------------------------------------------------------------------------
    public void SetActiveSellPedestal(SellPedestal pedestal)
    {
        activeSellPedestal = pedestal;
    }

    public void ClearActiveSellPedestal()
    {
        activeSellPedestal = null;
    }

    // Removes a dice from the active dice list if the sold item was an active dice
    public void TryRemoveActiveDice(ItemSlot slot)
    {
        if (slot == null)
            return;

        // If this slot is one of the active dice slots
        if (activeDiceSlots.Contains(slot))
        {
            // Remove the dice from the world
            DiceRollManager.Instance.RemoveDiceFromWorld(slot);

            // If this was the currently selected active dice, clear it
            if (activeDiceSlot == slot)
                activeDiceSlot = null;

            // Update UI
            UpdateActiveDiceUI();
        }
    }


    // -------------------------------------------------------------------------
    // Slot utilities
    // -------------------------------------------------------------------------
    public void DeselectAllSlots()
    {
        foreach (var slot in allSlots)
            slot?.DeselectSlot();
    }

    // -------------------------------------------------------------------------
    // Item management
    // -------------------------------------------------------------------------
    public void RemoveItem(ItemSlot slot, int amount)
    {
        if (slot == null)
            return;

        slot.quantity -= amount;

        if (slot.quantity <= 0)
        {
            // Clear the slot completely
            slot.ClearSlot();

            // If this was an active dice slot, clean up its state
            if (activeDiceSlots.Contains(slot))
            {
                DiceRollManager.Instance.RemoveDiceFromWorld(slot);

                if (activeDiceSlot == slot)
                    activeDiceSlot = null;

                UpdateActiveDiceUI();
            }
        }
        else
        {
            // Just refresh the UI if the slot still has items
            slot.RefreshUI();
        }
    }

    public int AddItem(BaseItemSO item, int quantity)
    {
        if (item is DiceSO)
            return AddItemToCategory(diceSlots, item, quantity);

        if (item is PermanentSO)
            return AddItemToCategory(permanentSlots, item, quantity);

        if (item is ConsumableSO)
            return AddItemToCategory(consumableSlots, item, quantity);

        return quantity;
    }

    private int AddItemToCategory(List<ItemSlot> slots, BaseItemSO item, int quantity)
    {
        foreach (var slot in slots)
        {
            if ((!slot.isFull && slot.itemName == item.itemName) || slot.quantity == 0)
            {
                int leftover = slot.AddItem(item.itemName, quantity, item.icon, item.itemDescription);
                if (leftover > 0)
                    return AddItemToCategory(slots, item, leftover);

                return 0;
            }
        }

        OptionPopupManager.Instance?.ShowInventoryFullPopup(
            item.itemName,
            quantity,
            item.icon,
            item.itemDescription
        );

        return quantity;
    }

    // -------------------------------------------------------------------------
    // Inventory UI
    // -------------------------------------------------------------------------
    public void ToggleInventory()
    {
        if (menuActivated)
            CloseInventory();
        else
            OpenInventory();
    }

    public void OpenInventory(bool pauseGame = true)
    {
        if (menuActivated) return;

        menuActivated = true;
        inventoryMenu?.SetActive(true);

        foreach (var slot in allSlots)
            slot?.RefreshUI();

        Time.timeScale = pauseGame ? 0f : 1f;
    }

    public void CloseInventory()
    {
        if (!menuActivated) return;

        menuActivated = false;
        inventoryMenu?.SetActive(false);
        DeselectAllSlots();


        Time.timeScale = 1f;

        activeSellPedestal = null;
    }

    // -------------------------------------------------------------------------
    // Active dice UI
    // -------------------------------------------------------------------------
    private void UpdateActiveDiceUI()
    {
        // Highlight all active dice slots that contain a die
        foreach (var slot in activeDiceSlots)
        {
            if (slot.quantity > 0)
                slot.SelectSlot();
            else
                slot.DeselectSlot();
        }

        // Build the text showing all active dice
        List<string> activeNames = new List<string>();

        foreach (var slot in activeDiceSlots)
        {
            if (slot.quantity > 0)
                activeNames.Add(slot.itemName);
        }

        if (activeNames.Count == 0)
        {
            activeDiceText.text = "Dados activos: ninguno";
        }
        else
        {
            activeDiceText.text = "Dados activos: " + string.Join(", ", activeNames);
        }
    }

}
