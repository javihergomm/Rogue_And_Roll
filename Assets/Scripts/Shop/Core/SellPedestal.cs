using UnityEngine;

/*
 * SellPedestal
 * ------------
 * Handles the selling flow using Ouija confirmation.
 *
 * Flow:
 * 1) When the playerObject enters this pedestal, the inventory opens in selling mode (game does not pause).
 * 2) When the playerObject clicks an item, the inventory closes immediately and a message-only popup appears.
 * 3) The playerObject walks to the YES/NO Ouija zones, which call HandleOuijaAnswer.
 */
public class SellPedestal : MonoBehaviour
{
    private bool sellingMode = false;

    // Item and slot pending to be sold after Ouija confirmation
    private BaseItemSO pendingItem;
    private ItemSlot pendingSlot;

    // Static reference so Ouija zones know which pedestal to notify
    public static SellPedestal currentSellPedestal;

    // True while waiting for the playerObject's YES/NO decision
    public bool isAwaitingDecision = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        sellingMode = true;

        // Register this pedestal so InventoryManager routes item clicks to it
        InventoryManager.Instance.SetActiveSellPedestal(this);

        // Open inventory without pausing the game
        InventoryManager.Instance.OpenInventory();

        Debug.Log("Sell pedestal activated. Inventory opened without pausing.");
    }

    /*
     * Called by InventoryManager when an item slot is clicked in selling mode.
     * Stores the pending sale, closes the inventory, and shows a message-only popup.
     */
    public void OnItemClicked(ItemSlot slot)
    {
        if (!sellingMode) return;
        if (slot == null || slot.Quantity <= 0) return;

        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.ItemName);
        if (item == null) return;

        // Store pending sale data
        pendingItem = item;
        pendingSlot = slot;

        // Mark this pedestal as active for Ouija confirmation
        currentSellPedestal = this;
        isAwaitingDecision = true;

        // Close inventory so the playerObject can walk to YES/NO
        InventoryManager.Instance.CloseInventory();

        // Show message-only popup
        if (OptionPopupManager.Instance != null)
        {
            OptionPopupManager.Instance.ShowMessage(
                "Quieres vender " + item.ItemName + " por " + item.SellPrice + " Pesetas?\n" +
                "Muevete al SI o al NO en el tablero."
            );
        }

        Debug.Log("Pending sale set for: " + item.ItemName + ". Inventory closed, awaiting Ouija decision.");
    }

    /*
     * Called by OuijaAnswerZone when the playerObject steps into YES or NO.
     * Completes or cancels the sale based on the answer.
     */
    public void HandleOuijaAnswer(OuijaAnswerZone.AnswerType answer)
    {
        Debug.Log("SellPedestal.HandleOuijaAnswer: " + answer);

        if (!isAwaitingDecision) return;
        if (pendingItem == null || pendingSlot == null)
        {
            isAwaitingDecision = false;
            return;
        }

        if (answer == OuijaAnswerZone.AnswerType.Yes)
        {
            // Remove from active dice if needed
            if (pendingItem is DiceSO)
            {
                InventoryManager.Instance.TryRemoveActiveDice(pendingSlot);
            }

            // Remove the item from the exact slot
            InventoryManager.Instance.RemoveItem(pendingSlot, 1);

            // Add gold
            StatManager.Instance.ChangeStat(StatType.Gold, pendingItem.SellPrice);

            Debug.Log("Sold: " + pendingItem.ItemName + " for " + pendingItem.SellPrice + " Pesetas.");
        }
        else
        {
            Debug.Log("Sale cancelled for " + pendingItem.ItemName);
        }

        // Hide popup
        if (OptionPopupManager.Instance != null)
            OptionPopupManager.Instance.HidePopup();

        // Clear state
        pendingItem = null;
        pendingSlot = null;
        isAwaitingDecision = false;
        if (currentSellPedestal == this)
            currentSellPedestal = null;

        EndSelling();
    }

    /*
     * Ends the selling process and closes the inventory if it is still open.
     */
    private void EndSelling()
    {
        sellingMode = false;
        InventoryManager.Instance.ClearActiveSellPedestal();
        InventoryManager.Instance.CloseInventory();

        Debug.Log("Selling mode ended.");
    }

    /*
     * Clears the pedestal when the playerObject exits the trigger,
     * but only if no Ouija decision is pending.
     */
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!isAwaitingDecision && currentSellPedestal == this)
        {
            currentSellPedestal = null;
            sellingMode = false;
            Debug.Log("Sell pedestal deactivated (no decision pending).");
        }
    }
}
