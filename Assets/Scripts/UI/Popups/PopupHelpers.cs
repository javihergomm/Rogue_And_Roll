using System.Collections.Generic;
using UnityEngine;

/*
 * PopupHelpers
 * ------------
 * High-level popup flows that combine UI (OptionPopupManager)
 * with game logic (Inventory, Characters, Shop, etc.).
 *
 * Keeps OptionPopupManager clean and UI-only.
 */
public static class PopupHelpers
{
    // -------------------------------------------------------------------------
    // CHARACTER SELECTION
    // -------------------------------------------------------------------------

    public static void ShowConfirmCharacterPopup(string characterName, System.Action onConfirm, System.Action onCancel)
    {
        var options = new List<PopupOption>
        {
            new PopupOption("Si", onConfirm, isConfirm: true),
            new PopupOption("No", onCancel)
        };

        OptionPopupManager.Instance.ShowPopup(
            "¿Estás seguro que quieres elegir " + characterName + " como tu personaje?",
            options
        );
    }

    // -------------------------------------------------------------------------
    // INVENTORY FULL
    // -------------------------------------------------------------------------

    public static void ShowInventoryFullPopup(string itemName, int quantity)
    {
        var options = new List<PopupOption>
        {
            new PopupOption("Si, reemplazar un objeto", () =>
            {
                InventoryManager.Instance.PrepareReplace(
                    InventoryManager.Instance.GetItemSO(itemName),
                    quantity
                );
                InventoryManager.Instance.OpenInventory();
            }, isConfirm: true),

            new PopupOption("No reemplazar", () => {})
        };

        OptionPopupManager.Instance.ShowPopup(
            "Inventario lleno. ¿Deseas reemplazar un objeto?",
            options
        );
    }

    // -------------------------------------------------------------------------
    // CONFIRM REPLACE
    // -------------------------------------------------------------------------

    public static void ShowConfirmReplacePopup(ItemSlot slot, System.Action onConfirm)
    {
        string message = "¿Seguro que quieres reemplazar el objeto '" + slot.ItemName + "' en este hueco?";

        var options = new List<PopupOption>
        {
            new PopupOption("Confirmar", onConfirm, isConfirm: true),
            new PopupOption("Cancelar", () => {})
        };

        OptionPopupManager.Instance.ShowPopup(message, options);
    }

    // -------------------------------------------------------------------------
    // REMOVE ITEMS
    // -------------------------------------------------------------------------

    public static void ShowRemoveItemPopup(ItemSlot[] itemSlots)
    {
        if (itemSlots == null || itemSlots.Length == 0)
            return;

        string itemName = itemSlots[0].ItemName;

        var options = new List<PopupOption>
        {
            new PopupOption("Eliminar 1 unidad", () =>
            {
                InventoryManager.Instance.RemoveItem(itemSlots[0], 1);
            }, isConfirm: true),

            new PopupOption("Eliminar todo", () =>
            {
                foreach (var slot in itemSlots)
                {
                    if (slot.Quantity > 0)
                        InventoryManager.Instance.RemoveItem(slot, slot.Quantity);
                }
            }),

            new PopupOption("Eliminar cantidad personalizada", () =>
            {
                int total = 0;
                foreach (var slot in itemSlots)
                    total += slot.Quantity;

                ShowNumberSliderPopup(itemSlots, total);
            }),

            new PopupOption("Cancelar", () => {})
        };

        OptionPopupManager.Instance.ShowPopup(
            "¿Cómo quieres eliminar " + itemName + "?",
            options
        );
    }

    // -------------------------------------------------------------------------
    // CUSTOM AMOUNT SLIDER
    // -------------------------------------------------------------------------

    private static void ShowNumberSliderPopup(ItemSlot[] itemSlots, int maxQuantity)
    {
        string itemName = itemSlots[0].ItemName;

        var options = new List<PopupOption>
        {
            new PopupOption("Confirmar", () => {}, isConfirm: true),
            new PopupOption("Cancelar", () => {})
        };

        OptionPopupManager.Instance.ShowPopup(
            "Selecciona la cantidad de " + itemName + " a eliminar:",
            options,
            useSlider: true,
            sliderMax: maxQuantity,
            onConfirmWithNumber: amount =>
            {
                int remaining = amount;

                foreach (var slot in itemSlots)
                {
                    if (remaining <= 0)
                        break;

                    int remove = Mathf.Min(slot.Quantity, remaining);
                    InventoryManager.Instance.RemoveItem(slot, remove);
                    remaining -= remove;
                }
            }
        );
    }

    // -------------------------------------------------------------------------
    // EXIT SHOP
    // -------------------------------------------------------------------------

    public static void ShowExitShopPopup(System.Action onConfirm, System.Action onCancel)
    {
        var options = new List<PopupOption>
        {
            new PopupOption("Si", onConfirm, isConfirm: true),
            new PopupOption("No", onCancel)
        };

        OptionPopupManager.Instance.ShowPopup(
            "¿Seguro que quieres salir de la tienda?",
            options
        );
    }
}
