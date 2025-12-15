using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * InventoryManager
 * ----------------
 * Central system for managing the player's inventory.
 */
public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory UI")]
    [SerializeField] private GameObject inventoryMenu;
    [SerializeField] private ItemSlot[] itemSlots;

    [Header("Item Data")]
    [SerializeField] private BaseItemSO[] itemSOs; 
    private bool menuActivated;

    // Lookup dictionary for items by name
    private Dictionary<string, BaseItemSO> itemLookup;

    // Replacement mode (when inventory is full)
    private bool waitingForReplace;
    private BaseItemSO pendingItem;
    private int pendingQuantity;

    // Active SellPedestal (if selling mode is active)
    private SellPedestal activeSellPedestal = null;

    private void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Build lookup dictionary
        itemLookup = new Dictionary<string, BaseItemSO>();
        foreach (var item in itemSOs)
        {
            if (!itemLookup.ContainsKey(item.itemName))
                itemLookup[item.itemName] = item;
        }

        // Hide inventory at start
        if (inventoryMenu != null)
            inventoryMenu.SetActive(false);
    }

    private void OnEnable()
    {
        StartCoroutine(RefreshSlotsNextFrame());
    }

    private IEnumerator RefreshSlotsNextFrame()
    {
        yield return null; // wait one frame

        if (inventoryMenu != null && inventoryMenu.activeSelf)
        {
            foreach (var slot in itemSlots)
                slot?.RefreshUI();
        }
    }

    public ItemSlot[] ItemSlots => itemSlots;

    // ---------------- Inventory open/close ----------------
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

        StartCoroutine(RefreshSlotsNextFrame());

        Time.timeScale = pauseGame ? 0f : 1f;
    }

    public void CloseInventory()
    {
        menuActivated = false;
        if (inventoryMenu != null)
            inventoryMenu.SetActive(false);

        Time.timeScale = 1f;

        waitingForReplace = false;
        pendingItem = null;
        activeSellPedestal = null;
    }

    // ---------------- Selling mode ----------------
    public void SetActiveSellPedestal(SellPedestal pedestal)
    {
        activeSellPedestal = pedestal;
    }

    public void ClearActiveSellPedestal()
    {
        activeSellPedestal = null;
    }

    // ---------------- Item usage ----------------
    public void UseItem(string itemName)
    {
        if (itemLookup.TryGetValue(itemName, out var item))
        {
            item.UseItem(); // polymorphic call
        }
        else
        {
            Debug.LogWarning("Item " + itemName + " not found.");
        }
    }

    // ---------------- Adding items ----------------
    public int AddItem(BaseItemSO item, int quantity)
    {
        return AddItem(item.itemName, quantity, item.icon, item.itemDescription);
    }

    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        foreach (var slot in itemSlots)
        {
            if ((!slot.isFull && slot.itemName == itemName) || slot.quantity == 0)
            {
                int leftover = slot.AddItem(itemName, quantity, itemSprite, itemDescription);

                if (leftover > 0)
                    return AddItem(itemName, leftover, itemSprite, itemDescription);

                return 0;
            }
        }

        // Inventory full
        if (OptionPopupManager.Instance != null)
        {
            OptionPopupManager.Instance.ShowInventoryFullPopup(itemName, quantity, itemSprite, itemDescription);
        }
        else
        {
            PrepareReplace(itemLookup[itemName], quantity);
            OpenInventory();
        }

        return quantity;
    }

    // ---------------- Slot click handler ----------------
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

        // Ensure SelectSlot calls UseItem
        slot.SelectSlot();
    }


    // ---------------- Replacement mode ----------------
    public void PrepareReplace(BaseItemSO item, int quantity)
    {
        waitingForReplace = true;
        pendingItem = item;
        pendingQuantity = quantity;
    }

    public void ReplaceInSlot(ItemSlot slot)
    {
        if (pendingItem == null) return;

        if (OptionPopupManager.Instance != null)
        {
            OptionPopupManager.Instance.ShowConfirmReplacePopup(slot, () =>
            {
                slot.ClearSlot();
                slot.AddItem(pendingItem.itemName, pendingQuantity, pendingItem.icon, pendingItem.itemDescription);
                waitingForReplace = false;
                pendingItem = null;
                CloseInventory();
            });
        }
        else
        {
            slot.ClearSlot();
            slot.AddItem(pendingItem.itemName, pendingQuantity, pendingItem.icon, pendingItem.itemDescription);
            waitingForReplace = false;
            pendingItem = null;
            CloseInventory();
        }
    }

    // ---------------- Removing items ----------------
    public void RemoveItem(string itemName, int quantity)
    {
        foreach (var slot in itemSlots)
        {
            if (slot.itemName == itemName && slot.quantity > 0)
            {
                int removeAmount = Mathf.Min(quantity, slot.quantity);
                slot.quantity -= removeAmount;
                quantity -= removeAmount;

                if (slot.quantity <= 0)
                    slot.ClearSlot();
                else
                    slot.RefreshUI();

                if (quantity <= 0) return;
            }
        }
    }

    // ---------------- Utility ----------------
    public void DeselectAllSlots()
    {
        foreach (var slot in itemSlots)
        {
            slot.selectedShader?.SetActive(false);
            slot.thisItemSelected = false;
        }
    }

    public BaseItemSO GetItemSO(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        if (itemLookup == null) return null;

        return itemLookup.TryGetValue(itemName, out var result) ? result : null;
    }
}
