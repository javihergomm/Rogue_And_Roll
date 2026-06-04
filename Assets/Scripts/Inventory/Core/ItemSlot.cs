using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * ItemSlot
 * --------
 * Represents a single inventory slot.
 * Handles:
 *   - Item data (name, sprite, quantity, description)
 *   - Slot UI updates
 *   - Slot selection
 *   - Pointer interactions (click, hover)
 *   - Drag & drop logic
 *   - Consumable placement on Spots / ColorSpots
 *
 * This class does NOT contain inventory logic.
 * All gameplay decisions are delegated to InventoryManager.
 */

public enum SlotType
{
    ActiveDice,
    Dice,
    Consumable,
    Permanent
}

public class ItemSlot : MonoBehaviour,
    IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    // Stored item data
    [SerializeField] private string itemName = "";
    [SerializeField] private int quantity = 0;
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private string itemDescription = "";
    [SerializeField] private Sprite emptySprite;

    [SerializeField] private SlotType slotType;

    private BaseItemSO itemSO;
    public BaseItemSO ItemSO => itemSO;
    public SlotType SlotType => slotType;

    // Called by InventorySlots.Initialize()
    public void SetSlotType(SlotType type)
    {
        slotType = type;
    }

    public string ItemName => itemName;
    public int Quantity => quantity;
    public Sprite ItemSprite => itemSprite;
    public string ItemDescription => itemDescription;

    // UI references
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject selectedShader;

    public bool ThisItemSelected { get; private set; }

    // Drag & drop helpers
    private CanvasGroup canvasGroup;
    private GameObject dragIcon;
    private Canvas dragCanvas;

    [SerializeField] private RectTransform inventoryPanel;

    private void Awake()
    {
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        RefreshUI();
    }

    /*
     * Adds an item to this slot (stacking).
     */
    public int AddItem(BaseItemSO item, int qty)
    {
        itemSO = item;
        itemName = item.ItemName;
        itemSprite = item.Icon;
        itemDescription = item.Description;

        quantity += qty;
        RefreshUI();
        return 0;
    }

    /*
     * Clears the slot completely.
     */
    public void ClearSlot()
    {
        itemSO = null;
        itemName = "";
        itemSprite = null;
        itemDescription = "";
        quantity = 0;
        ThisItemSelected = false;

        RefreshUI();
    }

    /*
     * Updates the slot UI based on current data.
     */
    public void RefreshUI()
    {
        if (itemImage != null)
            itemImage.sprite = quantity > 0 ? itemSprite : emptySprite;

        if (quantityText != null)
            quantityText.text = quantity > 1 ? quantity.ToString() : "";

        if (selectedShader != null)
            selectedShader.SetActive(ThisItemSelected);
    }

    /*
     * Selects this slot and deselects all others.
     */
    public void SelectSlot()
    {
        InventoryManager.Instance?.DeselectAllSlots();

        ThisItemSelected = true;

        if (selectedShader != null)
            selectedShader.SetActive(true);

        RefreshUI();
    }

    /*
     * Removes selection highlight.
     */
    public void DeselectSlot()
    {
        ThisItemSelected = false;

        if (selectedShader != null)
            selectedShader.SetActive(false);
    }

    /*
     * Handles click events and delegates to InventoryManager.
     */
    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryManager.Instance.HandleSlotClick(this);
    }

    /*
     * Begins dragging the item if allowed.
     */
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (quantity <= 0)
            return;

        BaseItemSO item = itemSO;

        // Slot type restrictions
        switch (slotType)
        {
            case SlotType.ActiveDice:
            case SlotType.Dice:
                if (item is not DiceSO)
                    return;
                break;

            case SlotType.Consumable:
                if (item is not ConsumableSO)
                    return;
                break;

            case SlotType.Permanent:
                if (item is not PermanentSO)
                    return;
                break;
        }

        // Permanent items cannot be dragged
        if (item is PermanentSO)
            return;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
        itemImage.enabled = false;

        CreateDragIcon();
        eventData.pointerDrag = gameObject;
    }

    /*
     * Moves the drag icon with the cursor.
     */
    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            dragIcon.transform.position = eventData.position;

        BaseItemSO item = itemSO;

        if (item is not ConsumableSO consumable)
            return;

        if (!consumable.CanBeUsedOnSpot)
            return;

        // Hide inventory when dragging outside the panel
        if (!RectTransformUtility.RectangleContainsScreenPoint(inventoryPanel, eventData.position))
        {
            if (InventoryManager.Instance.IsOpen)
                InventoryManager.Instance.HideInventorySoft();
        }
    }

    /*
     * Ends dragging and attempts to apply consumables to Spots.
     */
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        itemImage.enabled = true;

        if (dragIcon != null)
            Destroy(dragIcon);

        BaseItemSO item = itemSO;

        if (item is DiceSO)
            return;

        if (item is not ConsumableSO consumable)
            return;

        if (!consumable.CanBeUsedOnSpot)
            return;

        var target = GetClosestSpotToMouse(eventData);
        if (target == null)
            return;

        // Priority: ColorSpot first
        if (target is ColorSpot colorSpot)
        {
            InventoryManager.Instance.PlaceConsumableOnColorSpot(this, colorSpot);
            InventoryManager.Instance.CloseInventory();
            return;
        }

        // Otherwise normal Spot
        if (target is Spot spot)
        {
            InventoryManager.Instance.PlaceConsumableOnSpot(this, spot);
            InventoryManager.Instance.CloseInventory();
            return;
        }
    }

    /*
     * Handles dropping another slot onto this one.
     */
    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null)
            return;

        if (!draggedObject.TryGetComponent<ItemSlot>(out var from))
            return;

        BaseItemSO draggedItem = from.ItemSO;

        // Slot type restrictions
        switch (slotType)
        {
            case SlotType.ActiveDice:
            case SlotType.Dice:
                if (draggedItem is not DiceSO)
                    return;
                break;

            case SlotType.Consumable:
                if (draggedItem is not ConsumableSO)
                    return;
                break;

            case SlotType.Permanent:
                if (draggedItem is not PermanentSO)
                    return;
                break;
        }

        InventoryManager.Instance.HandleSlotDrop(from, this);
    }

    /*
     * Creates the icon that follows the cursor during drag.
     */
    private void CreateDragIcon()
    {
        if (dragCanvas == null)
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>();
            foreach (var c in canvases)
            {
                if (c.name == "GameCanvas")
                {
                    dragCanvas = c;
                    break;
                }
            }
        }

        if (dragCanvas == null)
            return;

        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(dragCanvas.transform, false);

        Image iconImage = dragIcon.AddComponent<Image>();
        iconImage.sprite = itemSprite;
        iconImage.raycastTarget = false;

        RectTransform rt = dragIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(64, 64);
    }

    /*
     * Finds the closest Spot or ColorSpot to the cursor.
     */
    private MonoBehaviour GetClosestSpotToMouse(PointerEventData eventData)
    {
        SpotController controller = Object.FindAnyObjectByType<SpotController>();
        List<MonoBehaviour> all = new();

        // ColorSpot first
        ColorSpot[] colorSpots = Object.FindObjectsByType<ColorSpot>();
        if (colorSpots != null && colorSpots.Length > 0)
            all.AddRange(colorSpots);

        // Normal Spots
        if (controller != null)
        {
            Spot[] normalSpots = controller.GetSpotsOrdered();
            if (normalSpots != null && normalSpots.Length > 0)
                all.AddRange(normalSpots);
        }

        if (all.Count == 0)
            return null;

        Ray ray = Camera.main.ScreenPointToRay(eventData.position);

        MonoBehaviour closest = null;
        float closestDistance = float.MaxValue;

        foreach (var s in all)
        {
            if (s == null)
                continue;

            float distance = Vector3.Cross(ray.direction, s.transform.position - ray.origin).magnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = s;
            }
        }

        return closest;
    }

    /*
     * Shows item description on hover.
     */
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (quantity <= 0)
            return;

        InventoryDescriptionUI ui = InventoryManager.Instance.DescriptionUI;
        ui.Show(itemName, itemDescription, itemSprite);
    }

    /*
     * Clears description on hover exit.
     */
    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryDescriptionUI ui = InventoryManager.Instance.DescriptionUI;
        ui.Clear();
    }
}
