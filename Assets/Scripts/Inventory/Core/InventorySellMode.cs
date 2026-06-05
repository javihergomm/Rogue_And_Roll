[System.Serializable]
public class InventorySellMode
{
    private SellPedestal activePedestal;

    // True when a SellPedestal is active
    public bool IsActive => activePedestal != null;

    // Called when the player enters a SellPedestal trigger
    public void Enable(SellPedestal pedestal)
    {
        activePedestal = pedestal;
    }

    // Called when the sale is finished or cancelled
    public void Disable()
    {
        activePedestal = null;
    }

    // Redirects the inventory click to the SellPedestal
    public void HandleClick(ItemSlot slot)
    {
        if (activePedestal != null)
            activePedestal.OnItemClicked(slot);
    }
}
