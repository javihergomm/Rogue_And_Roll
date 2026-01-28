using UnityEngine;

/*
 * InventorySellMode
 * -----------------
 * Handles selling items when interacting with a SellPedestal.
 */
[System.Serializable]
public class InventorySellMode
{
    private SellPedestal activePedestal;

    public bool IsActive => activePedestal != null;

    public void Enable(SellPedestal pedestal)
    {
        activePedestal = pedestal;
    }

    public void Disable()
    {
        activePedestal = null;
    }

    public void HandleClick(ItemSlot slot)
    {
        activePedestal?.OnItemClicked(slot);
    }
}

