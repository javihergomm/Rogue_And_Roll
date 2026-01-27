using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/*
 * InventoryManager
 * ----------------
 * Manages:
 *  - Inventory slots
 *  - Active dice slots
 *  - Item interactions (click, drag & drop)
 *  - Permanent effect activation/deactivation
 *  - Consumable usage
 *  - Sell pedestal routing
 *  - Inventory UI
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

    // Replace mode
    private bool waitingForReplace = false;
    private BaseItemSO pendingItem;
    private int pendingQuantity;

    // Selling mode
    private SellPedestal activeSellPedestal;

    // Inventory state
    private bool menuActivated = false;

    // Lookup table for item names
    private Dictionary<string, BaseItemSO> itemLookup;

    // Active dice selection
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

        // Build slot list
        allSlots.Clear();
        allSlots.AddRange(activeDiceSlots);
        allSlots.AddRange(diceSlots);
        allSlots.AddRange(permanentSlots);
        allSlots.AddRange(consumableSlots);

        if (inventoryMenu != null)
            inventoryMenu.SetActive(false);

        // Build item lookup
        itemLookup = new Dictionary<string, BaseItemSO>();
        foreach (var item in itemSOs)
            if (!itemLookup.ContainsKey(item.itemName))
                itemLookup[item.itemName] = item;

        if (activeDiceText != null)
            activeDiceText.text = "Ningun dado activo";
    }

    private void Start()
    {
        // Auto give a D6 at start
        DiceSO d6Item = itemSOs.OfType<DiceSO>().FirstOrDefault(d => d.itemName == "D6");
        if (d6Item == null) return;

        ItemSlot d6Slot = AddDiceToActiveSlots(d6Item);
        if (d6Slot == null) return;

        activeDiceSlot = d6Slot;
        UpdateActiveDiceUI();
        SyncActiveDiceSlot(d6Slot);
    }

    private void OnEnable()
    {
        LootBoxEvents.OnLootBoxOpened += HandleLootBoxOpened;
    }

    private void OnDisable()
    {
        LootBoxEvents.OnLootBoxOpened -= HandleLootBoxOpened;
    }


    // -------------------------------------------------------------------------
    // DICE MANAGEMENT
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

    // -------------------------------------------------------------------------
    // SLOT INTERACTION
    // -------------------------------------------------------------------------

    public BaseItemSO GetItemSO(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return null;
        return itemLookup.TryGetValue(itemName, out var result) ? result : null;
    }

    public void OnSlotClicked(ItemSlot slot)
    {
        // Ignore clicks inside shop exit UI
        if (slot.transform.root.GetComponent<ShopExitManager>() != null)
            return;

        bool wasSelected = slot.thisItemSelected;

        // Replace mode
        if (waitingForReplace)
        {
            ReplaceInSlot(slot);
            return;
        }

        // Selling mode
        if (activeSellPedestal != null)
        {
            activeSellPedestal.OnItemClicked(slot);
            return;
        }

        BaseItemSO item = GetItemSO(slot.itemName);

        // Dice or permanent: just select
        if (item is DiceSO || item is PermanentSO)
        {
            slot.SelectSlot();
            return;
        }

        // LootBox: behaves like a consumable
        if (item is LootBoxSO box)
        {
            if (!wasSelected)
            {
                slot.SelectSlot();
                return;
            }

            // Open the loot box (fires event)
            box.UseItem();

            // Remove the loot box
            RemoveItem(slot, 1);

            return;
        }

        // Consumable: use on second click
        if (item is ConsumableSO cons)
        {
            if (!wasSelected)
            {
                slot.SelectSlot();
                return;
            }

            cons.UseItem();          // Apply effects
            RemoveItem(slot, 1);     // Remove from inventory
            return;
        }
    }


    // -------------------------------------------------------------------------
    // DRAG & DROP
    // -------------------------------------------------------------------------

    public void HandleSlotDrop(ItemSlot from, ItemSlot to)
    {
        BaseItemSO item = GetItemSO(from.itemName);

        // Only dice can go into active dice slots
        if (activeDiceSlots.Contains(to) && !(item is DiceSO))
            return;

        // Swap everything using unified logic
        SwapSlots(from, to);
    }


    private void SwapSlots(ItemSlot a, ItemSlot b)
    {
        // Swap item data
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

        // Sync dice in world if needed
        SyncActiveDiceSlot(a);
        SyncActiveDiceSlot(b);

        // Update active dice UI
        UpdateActiveDiceUI();
    }


    // -------------------------------------------------------------------------
    // ADD / REMOVE ITEMS
    // -------------------------------------------------------------------------

    public int AddItem(BaseItemSO item, int quantity)
    {
        if (item is DiceSO)
            return AddItemToCategory(diceSlots, item, quantity);

        if (item is PermanentSO perm)
        {
            int leftover = AddItemToCategory(permanentSlots, item, quantity);

            if (leftover == 0)
                ActivatePermanentEffects(perm);

            return leftover;
        }

        // LootBox behaves like a consumable in inventory
        if (item is LootBoxSO)
            return AddItemToCategory(consumableSlots, item, quantity);

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

    public void RemoveItem(ItemSlot slot, int amount)
    {
        if (slot == null)
            return;

        BaseItemSO item = GetItemSO(slot.itemName);

        slot.quantity -= amount;

        if (slot.quantity <= 0)
        {
            slot.ClearSlot();

            // Remove permanent effects
            if (item is PermanentSO perm)
                DeactivatePermanentEffects(perm);

            // Remove dice from world
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

    // Handles loot box opening: removes the box and adds the reward item
    private void HandleLootBoxOpened(LootBoxSO box, BaseItemSO reward)
    {
        // Find the slot containing the loot box
        ItemSlot slot = allSlots.FirstOrDefault(s => s.itemName == box.itemName);

        if (slot != null)
            RemoveItem(slot, 1);   // Consume the loot box

        AddItem(reward, 1);        // Add the reward item
    }


    // -------------------------------------------------------------------------
    // PERMANENT EFFECTS
    // -------------------------------------------------------------------------

    private void ActivatePermanentEffects(PermanentSO perm)
    {
        if (perm.effects == null)
            return;

        foreach (var eff in perm.effects)
        {
            if (eff is BaseDiceEffect diceEff)
                CharacterEffectManager.Instance.AddDiceEffect(diceEff);

            else if (eff is BasePassiveEffect passiveEff)
                CharacterEffectManager.Instance.AddPassiveEffect(passiveEff);
        }
    }

    private void DeactivatePermanentEffects(PermanentSO perm)
    {
        if (perm.effects == null)
            return;

        foreach (var eff in perm.effects)
        {
            if (eff is BaseDiceEffect diceEff)
                CharacterEffectManager.Instance.RemoveDiceEffect(diceEff);

            else if (eff is BasePassiveEffect passiveEff)
                CharacterEffectManager.Instance.RemovePassiveEffect(passiveEff);
        }
    }

    // -------------------------------------------------------------------------
    // SELL PEDESTAL SUPPORT
    // -------------------------------------------------------------------------

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

            RefreshActiveDiceUI();
        }
    }

    // -------------------------------------------------------------------------
    // REPLACE MODE
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
    // UI
    // -------------------------------------------------------------------------

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

    public void DeselectAllSlots()
    {
        foreach (var slot in allSlots)
            slot?.DeselectSlot();
    }

    private void UpdateActiveDiceUI()
    {
        // Seleccionar o deseleccionar slots segun cantidad
        foreach (var slot in activeDiceSlots)
        {
            if (slot.quantity > 0)
                slot.SelectSlot();
            else
                slot.DeselectSlot();
        }

        List<string> activeInfo = new List<string>();

        // Comprobar si HideRollEffect esta activo globalmente
        bool hide = DiceRollManager.Instance.IsRollHidden();

        foreach (var slot in activeDiceSlots)
        {
            if (slot.quantity <= 0)
                continue;

            var rollInfo = DiceRollManager.Instance.GetRollInfo(slot);

            if (rollInfo.HasValue)
            {
                string baseText = hide ? "??" : rollInfo.Value.baseRoll.ToString();
                string finalText = hide ? "??" : rollInfo.Value.finalRoll.ToString();

                activeInfo.Add(slot.itemName + ": " + baseText + " -> " + finalText);
            }
            else
            {
                activeInfo.Add(slot.itemName + ": " + (hide ? "??" : "sin tirar"));
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
