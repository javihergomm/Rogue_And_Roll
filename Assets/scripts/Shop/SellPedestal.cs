using UnityEngine;

/*
 * SellPedestal
 * ------------
 * Selling flow with Ouija confirmation.
 * 1) Player enters this pedestal -> inventory opens in selling mode (without pausing the game).
 * 2) Player clicks an item -> inventory closes immediately, popup message-only appears.
 * 3) Player walks to SÍ/NO -> OuijaAnswerZone calls HandleOuijaAnswer.
 */
public class SellPedestal : MonoBehaviour
{
    private bool sellingMode = false;

    // Pending item info to sell after Ouija confirmation
    private BaseItemSO pendingItem;
    private ItemSlot pendingSlot;

    // Static reference so Ouija zones know which pedestal to notify
    public static SellPedestal currentSellPedestal;

    // Whether we are awaiting the player's SÍ/NO decision
    public bool isAwaitingDecision = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        sellingMode = true;

        // Register this pedestal in the InventoryManager (so it routes item clicks to us)
        InventoryManager.Instance.SetActiveSellPedestal(this);

        // IMPORTANT: open inventory WITHOUT pausing the game
        InventoryManager.Instance.OpenInventory(pauseGame: false);

        Debug.Log("Sell pedestal activated. Inventory opened without pausing.");
    }

    /*
     * Called by InventoryManager when a slot is clicked in selling mode.
     * Instead of showing buttons, we show a message-only popup and wait for Ouija answer.
     * Inventory is closed immediately after choosing the item.
     */
    public void OnItemClicked(ItemSlot slot)
    {
        if (!sellingMode) return;
        if (slot == null || slot.quantity <= 0) return;

        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.itemName);
        if (item == null) return;

        // Store pending sale data
        pendingItem = item;
        pendingSlot = slot;

        // Mark this pedestal as active for Ouija
        currentSellPedestal = this;
        isAwaitingDecision = true;

        // Close inventory so the player can move to SÍ/NO
        InventoryManager.Instance.CloseInventory();

        // Show message-only popup
        if (OptionPopupManager.Instance != null)
        {
            OptionPopupManager.Instance.ShowMessageOnly(
                $"¿Quieres vender {item.itemName} por {item.sellPrice} Pesetas?\n" +
                "Muévete al SÍ o al NO en el tablero."
            );
        }

        Debug.Log($"Pending sale set for: {item.itemName}. Inventory closed, awaiting Ouija decision.");
    }

    /*
     * Called by OuijaAnswerZone when the player steps into YES/NO.
     */
    public void HandleOuijaAnswer(OuijaAnswerZone.AnswerType answer)
    {
        Debug.Log("SellPedestal.HandleOuijaAnswer: " + answer);

        if (!isAwaitingDecision) return;
        if (pendingItem == null || pendingSlot == null) { isAwaitingDecision = false; return; }

        if (answer == OuijaAnswerZone.AnswerType.Yes)
        {
            // Remove one item
            InventoryManager.Instance.RemoveItem(pendingItem.itemName, 1);

            // Add gold
            StatManager.Instance.ChangeStat(StatType.Gold, pendingItem.sellPrice);

            Debug.Log($"Has vendido: {pendingItem.itemName} por {pendingItem.sellPrice} Pesetas.");
        }
        else
        {
            Debug.Log("Venta cancelada de " + pendingItem.itemName);
        }

        // Hide popup
        if (OptionPopupManager.Instance != null)
            OptionPopupManager.Instance.HidePopup();

        // Clear state and finish selling
        pendingItem = null;
        pendingSlot = null;
        isAwaitingDecision = false;
        if (currentSellPedestal == this)
            currentSellPedestal = null;

        EndSelling();
    }

    /*
     * Ends the selling process and closes inventory if still open.
     */
    private void EndSelling()
    {
        sellingMode = false;
        InventoryManager.Instance.ClearActiveSellPedestal();
        InventoryManager.Instance.CloseInventory();

        Debug.Log("Selling mode ended.");
    }

    /*
     * OnTriggerExit
     * -------------
     * Only clears the pedestal if we are NOT waiting for a decision.
     * This prevents losing the pending sale when the player bounces out of the collider.
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
