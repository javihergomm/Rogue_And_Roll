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
    public IReadOnlyList<ItemSlot> ItemSlots => slots.AllSlots;
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

        // 1. Cargar catálogo
        LoadItemCatalog();
        Debug.Log("Catalog count = " + itemCatalog.Count);

        // 2. Cargar unlocks
        Unlocks.Load();
        Debug.Log("Unlocks loaded: " + string.Join(",", Unlocks.GetAllUnlockedIDs()));

        // 3. Inicializar slots e inventario
        slots.Initialize();
        activeDice.Initialize(slots.ActiveDiceSlots);

        // 4. Inicializar CanvasGroup del menú
        if (inventoryMenu != null)
        {
            canvasGroup = inventoryMenu.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = inventoryMenu.AddComponent<CanvasGroup>();
        }

        // 5. Suscribirse a eventos
        LootBoxEvents.OnLootBoxOpened += HandleLootBoxReward;
    }
    private void Start()
    {
        // Añadir gafas destruidas cuando TODO está inicializado
        if (itemCatalog.TryGetValue("item_permanents_broken_glasses", out BaseItemSO testItem))
        {
            Debug.Log("[DEBUG] Añadiendo Gafas Destruidas al inventario en Start.");
            AddItem(testItem, 1);
        }
        else
        {
            Debug.LogError("[DEBUG] ERROR: No se encontró 'item_permanent_gafas_destruidas' en el catálogo.");
        }
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
                if (item == null)
                    continue;

                // DEBUG: print the real itemID of every ScriptableObject
                Debug.Log("[Catalog] Loaded item: " + item.itemID + " from folder " + folder);

                itemCatalog[item.itemID] = item;
            }
        }

        Debug.Log("[Catalog] Total items loaded = " + itemCatalog.Count);
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

    public BaseItemSO GetItemSO(string name)
    {
        if (itemCatalog.TryGetValue(name, out var item))
            return item;

        return null;
    }

    public void AddItem(BaseItemSO item, int qty)
    {
        slots.AddItem(item, qty);
        permanentEffects.TryActivate(item);

        if (item is ConsumableSO consumable && consumable.AutoUseOnPickup)
        {
            var ctx = new ConsumableContext();
            consumable.UseItem(ctx);

            if (ctx.WasUsed)
                slots.RemoveItemByName(consumable.ItemName, 1);
        }

        OnInventoryChanged?.Invoke();
    }

    public void RemoveItem(ItemSlot slot, int qty)
    {
        BaseItemSO item = slot.ItemSO;

        if (item is PermanentSO perm && perm.CannotBeUnequipped)
        {
            Debug.Log("This permanent item cannot be removed, sold or discarded.");
            return;
        }

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

        // Bloquear permanentes no desequipables
        if (item is PermanentSO perm && perm.CannotBeUnequipped)
        {
            Debug.Log("This permanent item cannot be unequipped.");
            return;
        }

        // Si estamos en modo venta
        if (sellMode.IsActive)
        {
            sellMode.HandleClick(slot);
            return;
        }

        // Si estamos esperando reemplazo
        if (slots.IsWaitingForReplace)
        {
            slots.ReplaceInSlot(slot);
            CloseInventory();
            return;
        }

        // Comportamiento normal
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


    public int GetFinalDiceNumber()
    {
        return DiceRollManager.Instance.GetTotalRoll();
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
        if (item == null)
            return;

        // Bloquear reemplazo de permanentes no desequipables
        if (item is PermanentSO perm && perm.CannotBeUnequipped)
        {
            Debug.Log("This permanent item cannot be replaced.");
            return;
        }

        slots.PrepareReplace(item, quantity);
        OpenInventory();
    }


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
        if (item is not ConsumableSO consumable)
            return;

        ConsumableContext ctx = new();

        if (target is ColorSpot colorSpot)
            ctx.TargetColorSpot = colorSpot;
        else if (target is Spot spot)
            ctx.TargetSpot = spot;
        else
            return;

        consumable.UseItem(ctx);

        if (!ctx.WasUsed)
            return;

        RemoveItem(slot, 1);
    }

    private void HandleLootBoxReward(LootBoxSO box, BaseItemSO reward)
    {
        AddItem(reward, 1);
    }
}
