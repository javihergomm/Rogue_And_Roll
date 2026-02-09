using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * ItemSlotDragHandler
 * -------------------
 * Handles dragging an inventory item icon outside the inventory panel.
 * Allows dropping the item onto board Spots.
 * If the drop is invalid, the icon returns to its original slot.
 */
public class ItemSlotDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemSlot Slot { get; private set; }

    [Header("UI References")]
    [SerializeField] private RectTransform inventoryPanel; // Inventory panel area
    [SerializeField] private Image iconImage;              // Icon image for raycast control

    private Transform originalParent;
    private Canvas canvas;

    public void Initialize(ItemSlot slot)
    {
        Slot = slot;
    }

    private void Awake()
    {
        // Cache the canvas so we can move the icon to the top level during drag
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Slot == null || Slot.Quantity == 0)
            return;

        originalParent = transform.parent;

        // Move the icon to the top of the canvas so it can leave the inventory panel
        transform.SetParent(canvas.transform.root, true);
        transform.SetAsLastSibling();

        // Disable raycast so the icon does not block drop detection
        iconImage.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Slot == null || Slot.Quantity == 0)
            return;

        // Follow the mouse
        transform.position = eventData.position;

        // If the icon leaves the inventory panel, close the inventory
        if (!RectTransformUtility.RectangleContainsScreenPoint(inventoryPanel, eventData.position))
        {
            InventoryManager.Instance.CloseInventory();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore raycast
        iconImage.raycastTarget = true;

        // If dropped on a Spot, SpotDropHandler will handle it
        if (eventData.pointerEnter != null &&
            eventData.pointerEnter.GetComponent<SpotDropHandler>() != null)
            return;

        // Otherwise, return the icon to its original slot
        transform.SetParent(originalParent, true);
        transform.localPosition = Vector3.zero;
    }
}
