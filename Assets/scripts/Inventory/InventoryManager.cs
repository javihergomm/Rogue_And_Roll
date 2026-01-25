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
        DiceSO d6Item = itemSOs.OfType<DiceSO>().FirstOrDefault(d => d.itemName == "D6");
        if (d6Item == null) return;

        ItemSlot d6Slot = AddDiceToActiveSlots(d6Item);
        if (d6Slot == null) return;

        activeDiceSlot = d6Slot;
        UpdateActiveDiceUI();

        SyncActiveDiceSlot(d6Slot);
    }

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

    public void SetActiveDice(ItemSlot sourceSlot, DiceSO dice)
    {
        ItemSlot targetSlot = activeDiceSlots.FirstOrDefault(s => s.thisItemSelected);
        if (targetSlot == null) return;

        bool sourceIsActive = activeDiceSlots.Contains(sourceSlot);

        if (targetSlot.quantity > 0)
        {
            string oldName = targetSlot.itemName;
            Sprite oldSprite = targetSlot.itemSprite;
            string oldDesc = targetSlot.itemDescription;

            targetSlot.ClearSlot();
            targetSlot.AddItem(dice.itemName, 1, dice.icon, dice.itemDescription);

            if (sourceSlot.quantity > 0)
            {
                sourceSlot.ClearSlot();
                sourceSlot.AddItem(oldName, 1, oldSprite, oldDesc);
            }
        }
        else
        {
            targetSlot.ClearSlot();
            targetSlot.AddItem(dice.itemName, 1, dice.icon, dice.itemDescription);

            if (sourceSlot.quantity > 0)
                sourceSlot.ClearSlot();
        }

        foreach (var slot in activeDiceSlots)
            SyncActiveDiceSlot(slot);

        activeDiceSlot = targetSlot;
        UpdateActiveDiceUI();
    }

    public BaseItemSO GetItemSO(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        return itemLookup.TryGetValue(itemName, out var result) ? result : null;
    }

    public void OnSlotClicked(ItemSlot slot)
    {
        if (slot.transform.root.GetComponent<ShopExitManager>() != null)
            return;

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

        if (item is DiceSO)
        {
            slot.SelectSlot();
            return;
        }

        if (item is PermanentSO)
        {
            slot.SelectSlot();
            return;
        }

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

        if (activeDiceSlots.Contains(to))
        {
            if (string.IsNullOrEmpty(from.itemName) || from.quantity <= 0)
                return;

            if (!(item is DiceSO))
                return;
        }

        if (item is DiceSO dice)
        {
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

    public void SetActiveSellPedestal(SellPedestal pedestal)
    {
        activeSellPedestal = pedestal;
    }

    public void ClearActiveSellPedestal()
    {
        activeSellPedestal = null;
    }

    public void TryRemoveActiveDice(ItemSlot slot)
    {
        if (slot == null)
            return;

        if (activeDiceSlots.Contains(slot))
        {
            DiceRollManager.Instance.RemoveDiceFromWorld(slot);

            if (activeDiceSlot == slot)
                activeDiceSlot = null;

            UpdateActiveDiceUI();
        }
    }

    public void DeselectAllSlots()
    {
        foreach (var slot in allSlots)
            slot?.DeselectSlot();
    }

    public void RemoveItem(ItemSlot slot, int amount)
    {
        if (slot == null)
            return;

        slot.quantity -= amount;

        if (slot.quantity <= 0)
        {
            slot.ClearSlot();

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

    public void ToggleInventory()
    {
        if (CharacterSelectManager.Instance != null &&
            CharacterSelectManager.Instance.IsAnySelectorUIOpen())
            return;

        if (menuActivated)
            CloseInventory();
        else
            OpenInventory();
    }

    public void OpenInventory(bool pauseGame = true)
    {
        if (CharacterSelectManager.Instance != null &&
            CharacterSelectManager.Instance.IsAnySelectorUIOpen())
            return;

        if (menuActivated)
            return;

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

    private void UpdateActiveDiceUI()
    {
        // Visual selection of active slots
        foreach (var slot in activeDiceSlots)
        {
            if (slot.quantity > 0)
                slot.SelectSlot();
            else
                slot.DeselectSlot();
        }

        List<string> activeInfo = new List<string>();

        foreach (var slot in activeDiceSlots)
        {
            if (slot.quantity <= 0)
                continue;

            // Get roll info from DiceRollManager
            var rollInfo = DiceRollManager.Instance.GetRollInfo(slot);

            if (rollInfo.HasValue)
            {
                int baseR = rollInfo.Value.baseRoll;
                int finalR = rollInfo.Value.finalRoll;

                // Example: "D6: 4 -> 6"
                activeInfo.Add(slot.itemName + ": " + baseR + " -> " + finalR);
            }
            else
            {
                // Not rolled yet
                activeInfo.Add(slot.itemName + ": sin tirar");
            }
        }

        if (activeInfo.Count == 0)
            activeDiceText.text = "Dados activos: ninguno";
        else
            activeDiceText.text = "Dados activos: " + string.Join(" | ", activeInfo);
    }
    public void RefreshActiveDiceUI()
    {
        UpdateActiveDiceUI();
    }


}
