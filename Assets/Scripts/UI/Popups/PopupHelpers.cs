using System.Collections.Generic;
using UnityEngine;

/*
 * PopupHelpers
 * ------------
 * High-level helper class that connects UI popups (OptionPopupManager)
 * with gameplay logic (Inventory, Characters, Shop, etc.).
 *
 * Purpose:
 *   - Keep OptionPopupManager strictly UI-only.
 *   - Centralize all popup flows that require game logic.
 *   - Provide clean, readable, reusable popup patterns.
 */
public static class PopupHelpers
{
    // -------------------------------------------------------------------------
    // CHARACTER SELECTION
    // -------------------------------------------------------------------------

    /*
     * Shows a confirmation popup when selecting a character.
     * onConfirm -> executed if the player accepts.
     * onCancel  -> executed if the player declines.
     */
    public static void ShowConfirmCharacterPopup(string characterName, System.Action onConfirm, System.Action onCancel)
    {
        var options = new List<PopupOption>
        {
            new("Si", onConfirm, isConfirm: true),
            new("No", onCancel)
        };

        OptionPopupManager.Instance.ShowPopup(
            "Estas seguro que quieres elegir " + characterName + " como tu personaje?",
            options
        );
    }

    // -------------------------------------------------------------------------
    // INVENTORY FULL
    // -------------------------------------------------------------------------

    /*
     * Shows a popup when the inventory is full.
     * Allows the player to replace an existing item or cancel.
     */
    public static void ShowInventoryFullPopup(string itemName, int quantity)
    {
        var options = new List<PopupOption>
        {
            new("Si, reemplazar un objeto", () =>
            {
                // Prepare replace mode with the item that could not be added
                InventoryManager.Instance.PrepareReplace(
                    InventoryManager.Instance.GetItemSO(itemName),
                    quantity
                );

                // Open inventory so the player can choose a slot to replace
                InventoryManager.Instance.OpenInventory();
            }, isConfirm: true),

            new("No reemplazar", () => {})
        };

        OptionPopupManager.Instance.ShowPopup(
            "Inventario lleno. Deseas reemplazar un objeto?",
            options
        );
    }

    // -------------------------------------------------------------------------
    // CONFIRM REPLACE
    // -------------------------------------------------------------------------

    /*
     * Shows a confirmation popup before replacing an item in a slot.
     */
    public static void ShowConfirmReplacePopup(ItemSlot slot, System.Action onConfirm)
    {
        string message = "Seguro que quieres reemplazar el objeto '" + slot.ItemName + "' en este hueco?";

        var options = new List<PopupOption>
        {
            new("Confirmar", onConfirm, isConfirm: true),
            new("Cancelar", () => {})
        };

        OptionPopupManager.Instance.ShowPopup(message, options);
    }

    // -------------------------------------------------------------------------
    // EXIT SHOP
    // -------------------------------------------------------------------------

    /*
     * Shows a confirmation popup when exiting the shop.
     */
    public static void ShowExitShopPopup(System.Action onConfirm, System.Action onCancel)
    {
        var options = new List<PopupOption>
        {
            new("Si", onConfirm, isConfirm: true),
            new("No", onCancel)
        };

        OptionPopupManager.Instance.ShowPopup(
            "Seguro que quieres salir de la tienda?",
            options
        );
    }

    // -------------------------------------------------------------------------
    // UNLOCK POPUP
    // -------------------------------------------------------------------------

    /*
     * Shows a short timed popup when unlocking a new item.
     * Duration intentionally short to avoid blocking gameplay flow.
     */
    public static void ShowUnlockPopup(string itemName)
    {
        OptionPopupManager.Instance.ShowTimedMessage(
            "Nuevo objeto desbloqueado!\n" + itemName,
            2f // short duration to avoid blocking gameplay
        );
    }
}
