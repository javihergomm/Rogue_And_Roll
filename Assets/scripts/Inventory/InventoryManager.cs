using System.Collections.Generic;
using System.Linq;
using TMPro; // for the optional active dice label
using UnityEngine;

/*
 * InventoryManager
 * ----------------
 * Central system for managing the player's inventory.
 * Responsibilities:
 * - Manages dice, permanent, and consumable item slots.
 * - Handles item selection and interaction logic.
 * - Coordinates with DiceRollManager to spawn and roll dice.
 * - Updates UI to highlight the active dice slot and label.
 * - Provides methods for adding, removing, and replacing items.
 * - Controls opening and closing of the inventory interface.
 */
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    [Header("Inventory Slots")]
    [SerializeField] private List<ItemSlot> diceSlots;
    [SerializeField] private List<ItemSlot> permanentSlots;
    [SerializeField] private List<ItemSlot> consumableSlots;

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private TMP_Text activeDiceText; // optional: shows active dice name

    [Header("Item Data")]
    [SerializeField] private BaseItemSO[] itemSOs;

    // Consolidated list (used for global deselection and item lookup)
    private readonly List<ItemSlot> allSlots = new List<ItemSlot>();
    public IReadOnlyList<ItemSlot> ItemSlots => allSlots;

    // State flags
    private bool waitingForReplace = false;
    private bool menuActivated = false;

    // Active sell pedestal
    private SellPedestal activeSellPedestal;

    // Replacement mode
    private BaseItemSO pendingItem;
    private int pendingQuantity;

    // Lookup dictionary
    private Dictionary<string, BaseItemSO> itemLookup;

    // Track the currently active dice slot
    private ItemSlot activeDiceSlot;
    public ItemSlot ActiveDiceSlot => activeDiceSlot;
    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Build consolidated slot list
        allSlots.Clear();
        if (diceSlots != null) allSlots.AddRange(diceSlots);
        if (permanentSlots != null) allSlots.AddRange(permanentSlots);
        if (consumableSlots != null) allSlots.AddRange(consumableSlots);

        // Hide inventory at start
        if (inventoryMenu != null)
            inventoryMenu.SetActive(false);

        // Build item lookup
        itemLookup = new Dictionary<string, BaseItemSO>();
        foreach (var item in itemSOs)
        {
            if (!itemLookup.ContainsKey(item.itemName))
                itemLookup[item.itemName] = item;
        }

        // Initialize active dice label
        if (activeDiceText != null)
            activeDiceText.text = "Ningun dado activo";
    }

    private void Start()
    {
        // Initialize inventory with a D6 once everything is ready
        DiceSO d6Item = itemSOs.OfType<DiceSO>().FirstOrDefault(d => d.itemName == "D6");
        if (d6Item == null) return;

        AddItem(d6Item, 1);

        // Find the slot where the D6 was placed
        ItemSlot d6Slot = diceSlots.FirstOrDefault(s => s.itemName == d6Item.itemName && s.quantity > 0);
        if (d6Slot == null) return;

        // Mark as active dice
        activeDiceSlot = d6Slot;

        // Update UI state
        UpdateActiveDiceUI();

        // Spawn the dice immediately
        if (DiceRollManager.Instance != null)
        {
            DiceRollManager.Instance.SpawnDice(d6Item, 0, d6Slot);
        }

        Debug.Log("[InventoryManager] Inventory initialized with a D6 active and spawned.");
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

        slot.SelectSlot();

        BaseItemSO item = GetItemSO(slot.itemName);
        if (item is DiceSO dice)
        {
            Debug.Log("[Inventory] Dice selected: " + dice.itemName);

            // Track and update active dice UI
            activeDiceSlot = slot;
            UpdateActiveDiceUI();

            if (DiceRollManager.Instance != null)
            {
                DiceRollManager.Instance.SpawnDice(dice, 0, slot);
            }
            else
            {
                Debug.LogWarning("[Inventory] DiceRollManager not present in scene.");
            }
        }
        else if (item is PermanentSO perm)
        {
            Debug.Log("[Inventory] Permanent selected: " + perm.itemName);
            // TODO: implement permanent equip logic
        }
        else if (item is ConsumableSO cons)
        {
            Debug.Log("[Inventory] Consumable used: " + cons.itemName);
            if (StatManager.Instance != null)
            {
                StatManager.Instance.TryUseItem(cons);
            }
            else
            {
                Debug.LogWarning("[Inventory] StatManager not present in scene.");
            }
        }
        else
        {
            Debug.LogWarning("[Inventory] No valid item found for slot: " + slot.itemName);
        }
    }
    // -------------------------------------------------------------------------
    // Active dice UI update
    // -------------------------------------------------------------------------
    private void UpdateActiveDiceUI()
    {
        // Clear visual highlight on all dice slots
        foreach (var slot in diceSlots)
        {
            if (slot != null)
                slot.DeselectSlot();
        }

        // Highlight the active dice slot and update label
        if (activeDiceSlot != null)
        {
            activeDiceSlot.SelectSlot();

            if (activeDiceText != null)
                activeDiceText.text = "Dado activo: " + activeDiceSlot.itemName;
        }
        else
        {
            if (activeDiceText != null)
                activeDiceText.text = "Ningún dado activo";
        }
    }
    // -------------------------------------------------------------------------
    // Replacement mode
    // -------------------------------------------------------------------------
    private void ReplaceInSlot(ItemSlot slot)
    {
        Debug.Log("[Inventory] Replace logic triggered for slot: " + slot.itemName);
        // TODO: implement replace logic
        waitingForReplace = false;
    }

    public void PrepareReplace(BaseItemSO item, int quantity)
    {
        waitingForReplace = true;
        pendingItem = item;
        pendingQuantity = quantity;
        Debug.Log("[InventoryManager] PrepareReplace called for " + item.itemName + " x" + quantity);
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
        Debug.Log("[InventoryManager] Active sell pedestal cleared.");
    }
    // -------------------------------------------------------------------------
    // Slot utilities
    // -------------------------------------------------------------------------
    public void DeselectAllSlots()
    {
        foreach (var slot in allSlots)
        {
            if (slot != null)
                slot.DeselectSlot();
        }
        Debug.Log("[Inventory] All slots deselected.");
    }

    // -------------------------------------------------------------------------
    // Item management
    // -------------------------------------------------------------------------
    public void RemoveItem(string itemName, int amount)
    {
        // Loop through all inventory slots to find the matching item
        foreach (var slot in allSlots)
        {
            if (slot != null && slot.itemName == itemName && slot.quantity > 0)
            {
                // Subtract only the requested amount
                slot.quantity -= amount;

                if (slot.quantity > 0)
                {
                    // If there are still items left, refresh the UI to show the new quantity
                    slot.RefreshUI();
                }
                else
                {
                    // If no items remain, clear the slot completely
                    slot.ClearSlot();

                    // If we cleared an active dice, reset the active dice UI
                    if (slot == activeDiceSlot)
                    {
                        activeDiceSlot = null;
                        UpdateActiveDiceUI();
                    }
                }

                Debug.Log("[Inventory] Removed " + amount + " of " + itemName);
                return;
            }
        }

        // If no matching item was found, log a warning
        Debug.LogWarning("[Inventory] Tried to remove item not found: " + itemName);
    }

    public int AddItem(BaseItemSO item, int quantity)
    {
        if (item is DiceSO)
            return AddItemToCategory(diceSlots, item, quantity);
        else if (item is PermanentSO)
            return AddItemToCategory(permanentSlots, item, quantity);
        else if (item is ConsumableSO)
            return AddItemToCategory(consumableSlots, item, quantity);

        Debug.LogWarning("[Inventory] Item type not recognized: " + item.itemName);
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

        // Category full -> trigger replace mode
        if (OptionPopupManager.Instance != null)
        {
            OptionPopupManager.Instance.ShowInventoryFullPopup(item.itemName, quantity, item.icon, item.itemDescription);
        }
        else
        {
            PrepareReplace(item, quantity);
            OpenInventory();
        }

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
        menuActivated = true;
        if (inventoryMenu != null)
            inventoryMenu.SetActive(true);

        foreach (var slot in allSlots)
        {
            if (slot != null)
                slot.RefreshUI();
        }

        Time.timeScale = pauseGame ? 0f : 1f;
        Debug.Log("[InventoryManager] Inventory opened.");
    }


    public void CloseInventory()
    {
        menuActivated = false;
        if (inventoryMenu != null)
            inventoryMenu.SetActive(false);

        Time.timeScale = 1f;
        waitingForReplace = false;
        activeSellPedestal = null;

        Debug.Log("[InventoryManager] Inventory closed.");
    }
    // -------------------------------------------------------------------------
    // Debugging utility
    // -------------------------------------------------------------------------
    public void DebugSlotVisuals()
    {
        Debug.Log("=== INVENTORY SLOT DEBUG ===");
        foreach (var slot in allSlots)
        {
            if (slot == null) continue;

            string spriteName = slot.itemSprite != null ? slot.itemSprite.name : "null";
            string imageSpriteName = slot.GetComponentInChildren<UnityEngine.UI.Image>()?.sprite != null
                ? slot.GetComponentInChildren<UnityEngine.UI.Image>().sprite.name
                : "null";

            Debug.Log(
                $"Slot: {slot.name} | Item: {slot.itemName} | Qty: {slot.quantity} | " +
                $"itemSprite: {spriteName} | itemImage.sprite: {imageSpriteName}"
            );
        }
        Debug.Log("============================");
    }

}
