using UnityEngine;
using System.Collections.Generic;

/*
 * SellPedestal
 * ------------
 * Handles the entire selling process.
 * When the player enters this trigger, the inventory opens in selling mode.
 * When the player clicks an item, this script receives the callback and manages the sale.
 */
public class SellPedestal : MonoBehaviour
{
    private bool sellingMode = false;

    private void OnTriggerEnter(Collider other)
    {
        // Only react to the player
        if (!other.CompareTag("Player")) return;

        // Start selling mode
        sellingMode = true;

        // Register this pedestal in the InventoryManager
        InventoryManager.Instance.SetActiveSellPedestal(this);

        // Open inventory
        InventoryManager.Instance.OpenInventory();
    }

    /*
     * Called by InventoryManager when a slot is clicked.
     */
    public void OnItemClicked(ItemSlot slot)
    {
        if (!sellingMode) return;
        if (slot == null || slot.quantity <= 0) return;

        ItemSO item = InventoryManager.Instance.GetItemSO(slot.itemName);
        if (item == null) return;

        // Ask for confirmation (SPANISH)
        OptionPopupManager.Instance.ShowPopup(
            "Quieres vender " + item.itemName + " por " + item.sellPrice + " Pesetas?",
            new Dictionary<string, System.Action> {
                { "Si", () => {
                    // Remove one item
                    InventoryManager.Instance.RemoveItem(item.itemName, 1);

                    // Add gold
                    StatManager.Instance.ChangeStat(ItemSO.StatType.gold, item.sellPrice);

                    // End selling without extra notifications
                    EndSelling();
                }},
                { "No", () => {
                    // Simply cancel without showing another popup
                    EndSelling();
                }}
            }
        );
    }

    /*
     * Ends the selling process and closes the inventory.
     */
    private void EndSelling()
    {
        sellingMode = false;
        InventoryManager.Instance.ClearActiveSellPedestal();
        InventoryManager.Instance.CloseInventory();
    }
}
