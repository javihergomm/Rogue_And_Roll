using UnityEngine;

/*
 * SellPedestal
 * ------------
 * Handles the selling flow using Ouija confirmation.
 * When the player enters the pedestal, the inventory opens in selling mode.
 * After selecting an item, the player must confirm the sale by moving to YES or NO.
 */
public class SellPedestal : MonoBehaviour
{
    // Global selling flag to ensure only one selling interaction at a time
    public static bool sellingMode = false;

    // Item and slot pending confirmation
    private BaseItemSO pendingItem;
    private ItemSlot pendingSlot;

    // Reference to the pedestal currently waiting for YES/NO
    public static SellPedestal currentSellPedestal;

    // True while waiting for the player to confirm the sale
    public bool isAwaitingDecision = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Prevent selling if another selling interaction is active
        if (sellingMode)
            return;

        // Prevent selling if a buying interaction is active
        if (ShopPedestalRandomizer.buyingMode)
            return;

        // Activate selling mode
        sellingMode = true;
        currentSellPedestal = this;

        // Enable selling mode in the inventory
        InventoryManager.Instance.SetActiveSellPedestal(this);

        // Open inventory without pausing the game
        InventoryManager.Instance.OpenInventory();

    }

    /*
     * Called when the player clicks an item in selling mode.
     * Stores the pending sale and shows the confirmation popup.
     */
    public void OnItemClicked(ItemSlot slot)
    {
        if (!sellingMode)
            return;

        if (slot == null || slot.Quantity <= 0)
            return;

        BaseItemSO item = slot.ItemSO;
        if (item == null)
            return;

        pendingItem = item;
        pendingSlot = slot;

        isAwaitingDecision = true;

        // Close inventory so the player can walk to YES/NO
        InventoryManager.Instance.CloseInventory();

        if (OptionPopupManager.Instance != null)
        {
            OptionPopupManager.Instance.ShowMessage(
                "Quieres vender " + item.ItemName + " por " + item.SellPrice + " Pesetas?\n" +
                "Muevete al SI o al NO en el tablero."
            );
        }

    }

    /*
     * Called when the player enters the YES or NO Ouija zone.
     * Completes or cancels the sale.
     */
    public void HandleOuijaAnswer(OuijaAnswerZone.AnswerType answer)
    {
        if (!isAwaitingDecision)
            return;

        if (pendingItem == null || pendingSlot == null)
        {
            isAwaitingDecision = false;
            return;
        }

        if (answer == OuijaAnswerZone.AnswerType.Yes)
        {
            // Remove from active dice if needed
            if (pendingItem is DiceSO)
                InventoryManager.Instance.TryRemoveActiveDice(pendingSlot);

            // Remove the item
            InventoryManager.Instance.RemoveItem(pendingSlot, 1);

            // Add gold
            StatManager.Instance.ChangeStat(StatType.Gold, pendingItem.SellPrice);
 
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
     * Ends the selling process and closes the inventory.
     */
    private void EndSelling()
    {
        sellingMode = false;

        InventoryManager.Instance.ClearActiveSellPedestal();
        InventoryManager.Instance.CloseInventory();

    }

    /*
     * Clears the pedestal when the player exits the trigger.
     */
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!isAwaitingDecision && currentSellPedestal == this)
        {
            currentSellPedestal = null;
            sellingMode = false;

        }
    }
}
