using UnityEngine;
using UnityEngine.EventSystems;

/*
 * SpotDropHandler
 * ----------------
 * Receives dragged items dropped onto a board Spot.
 * If the dragged UI belongs to an inventory slot,
 * the InventoryManager places the consumable on this Spot.
 */
public class SpotDropHandler : MonoBehaviour, IDropHandler
{
    private Spot spot;

    private void Awake()
    {
        // Cache the Spot component on this GameObject
        spot = GetComponent<Spot>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // The UI element being dragged
        var dragged = eventData.pointerDrag;
        if (dragged == null)
            return;

        // Check if the dragged object is an inventory item icon
        var dragHandler = dragged.GetComponent<ItemSlotDragHandler>();
        if (dragHandler == null)
            return;

        // Place the consumable on this spot
        InventoryManager.Instance.PlaceConsumableOnSpot(dragHandler.Slot, spot);
    }
}
