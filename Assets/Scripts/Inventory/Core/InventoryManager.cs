using System;
using System.Collections.Generic;
using UnityEngine;

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

    private Dictionary<string, BaseItemSO> itemCatalog = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadItemCatalog();
        Unlocks.Load();

        slots.Initialize();
        activeDice.Initialize(slots.ActiveDiceSlots);

        if (inventoryMenu != null)
        {
            canvasGroup = inventoryMenu.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = inventoryMenu.AddComponent<CanvasGroup>();
        }

        LootBoxEvents.OnLootBoxOpened += HandleLootBoxReward;
    }

    private void Start()
    {
        // IDs exactos de tus objetos
        string idCatan = "item_consumables_catan_bridge";
        string idD4 = "item_dice_d4";
        string idLimo = "item_special_slime_even_only";

        // Obtener objetos del catálogo
        BaseItemSO catan = GetItemSO(idCatan);
        BaseItemSO d4 = GetItemSO(idD4);
        BaseItemSO slime = GetItemSO(idLimo);

        // Añadir Puente del Catán
        if (catan != null)
        {
            AddItem(catan, 1);
            Debug.Log("Añadido Puente del Catán al iniciar.");
        }
        else Debug.LogError("No se encontró " + idCatan);

        // Añadir D4
        if (d4 != null)
        {
            AddItem(d4, 1);
            Debug.Log("Añadido D4 al iniciar.");
        }
        else Debug.LogError("No se encontró " + idD4);

        // Añadir Limo
        if (slime != null)
        {
            AddItem(slime, 1);
            Debug.Log("Añadido Limo al iniciar.");
        }
        else Debug.LogError("No se encontró " + idLimo);
    }


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

    public BaseItemSO GetItemSO(string id)
    {
        if (itemCatalog.TryGetValue(id, out var item))
            return item;

        return null;
    }

    public void AddItem(BaseItemSO item, int qty)
    {
        slots.AddItem(item, qty);
        permanentEffects.TryActivate(item);

        if (item is ConsumableSO cons && cons.AutoUseOnPickup)
        {
            var ctx = new ConsumableContext();
            cons.UseItem(ctx);

            if (ctx.WasUsed)
                slots.RemoveItemByName(cons.ItemName, 1);
        }

        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(ItemSlot slot, int qty)
    {
        BaseItemSO item = slot.ItemSO;

        if (item is PermanentSO perm && perm.CannotBeUnequipped)
            return;

        slots.RemoveItem(slot, qty);
        permanentEffects.TryDeactivate(item);

        if (activeDice.Contains(slot))
            activeDice.SyncSlot(slot);

        OnInventoryChanged?.Invoke();
    }

    public void HandleSlotClick(ItemSlot slot)
    {
        if (slot == null)
            return;

        BaseItemSO item = slot.ItemSO;

        if (item is PermanentSO perm && perm.CannotBeUnequipped)
            return;

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
        if (from == null || to == null)
            return;

        BaseItemSO item = from.ItemSO;

        if (activeDice.Contains(to) && item is not DiceSO)
            return;

        slots.SwapSlots(from, to);

        activeDice.SyncSlot(from);
        activeDice.SyncSlot(to);

        OnActiveDiceChanged?.Invoke();
    }

    // ---------------------------------------------------------
    // ACTIVE DICE API (usado por DiceRoller, Movement, Spawner)
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
    // LOOTBOX
    // ---------------------------------------------------------

    private void HandleLootBoxReward(LootBoxSO box, BaseItemSO reward)
    {
        AddItem(reward, 1);
    }
}
