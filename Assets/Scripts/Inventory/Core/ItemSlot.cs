using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * ItemSlot
 * --------
 * Represents a single inventory slot.
 * Handles UI, selection, and drag interactions.
 * Dice can only be dragged inside the inventory.
 * Consumables can only be dragged when the inventory is CLOSED
 * and dropped onto the board (Spot).
 */
public class ItemSlot : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    // -------------------------------------------------------------------------
    // DATA
    // -------------------------------------------------------------------------

    [SerializeField] private string itemName = "";
    [SerializeField] private int quantity = 0;
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private bool isFull = false;
    [SerializeField] private string itemDescription = "";
    [SerializeField] private Sprite emptySprite;

    [SerializeField] private int maxNumberOfItems = 10;

    public string ItemName => itemName;
    public int Quantity => quantity;
    public bool IsFull => isFull;
    public string ItemDescription => itemDescription;
    public Sprite ItemSprite => itemSprite;

    // -------------------------------------------------------------------------
    // UI
    // -------------------------------------------------------------------------

    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;

    [Header("Description UI")]
    [SerializeField] private Image itemDescriptionImage;
    [SerializeField] private TMP_Text itemDescriptionNameText;
    [SerializeField] private TMP_Text itemDescriptionText;

    [Header("Selection Highlight")]
    [SerializeField] private GameObject selectedShader;

    public bool thisItemSelected { get; private set; }

    // -------------------------------------------------------------------------
    // DRAG & DROP
    // -------------------------------------------------------------------------

    private CanvasGroup canvasGroup;
    private GameObject dragIcon;
    private Canvas dragCanvas;

    private void Awake()
    {
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.enabled = false;
        }

        if (selectedShader != null)
            selectedShader.SetActive(false);

        if (itemImage != null)
            itemImage.sprite = emptySprite;
    }

    // -------------------------------------------------------------------------
    // ITEM MANAGEMENT
    // -------------------------------------------------------------------------

    public int AddItem(string name, int qty, Sprite sprite, string description)
    {
        if (isFull)
            return qty;

        itemName = name;
        itemSprite = sprite;
        itemDescription = description;

        itemImage.sprite = sprite;

        quantity += qty;

        if (quantity >= maxNumberOfItems)
        {
            int extra = quantity - maxNumberOfItems;
            quantity = maxNumberOfItems;
            isFull = true;
            RefreshUI();
            return extra;
        }

        RefreshUI();
        return 0;
    }

    public void ClearSlot()
    {
        quantity = 0;
        isFull = false;
        thisItemSelected = false;

        itemName = "";
        itemSprite = null;
        itemDescription = "";

        if (quantityText != null)
        {
            quantityText.text = "";
            quantityText.enabled = false;
        }

        if (itemImage != null)
            itemImage.sprite = emptySprite;

        if (itemDescriptionNameText != null) itemDescriptionNameText.text = "";
        if (itemDescriptionText != null) itemDescriptionText.text = "";
        if (itemDescriptionImage != null) itemDescriptionImage.sprite = emptySprite;

        if (selectedShader != null)
            selectedShader.SetActive(false);
    }

    public void RefreshUI()
    {
        if (quantityText != null)
        {
            if (quantity > 0)
            {
                quantityText.text = quantity.ToString();
                quantityText.enabled = true;
            }
            else
            {
                quantityText.text = "";
                quantityText.enabled = false;
            }
        }

        if (itemImage != null)
            itemImage.sprite = quantity > 0 ? itemSprite : emptySprite;
    }

    // -------------------------------------------------------------------------
    // SELECTION
    // -------------------------------------------------------------------------

    public void SelectSlot()
    {
        InventoryManager.Instance?.DeselectAllSlots();

        if (selectedShader != null)
            selectedShader.SetActive(true);

        thisItemSelected = true;

        if (itemDescriptionNameText != null)
            itemDescriptionNameText.text = itemName;

        if (itemDescriptionText != null)
            itemDescriptionText.text = itemDescription;

        if (itemDescriptionImage != null)
            itemDescriptionImage.sprite = itemSprite ?? emptySprite;
    }

    public void DeselectSlot()
    {
        if (selectedShader != null)
            selectedShader.SetActive(false);

        thisItemSelected = false;
    }

    // -------------------------------------------------------------------------
    // CLICK HANDLING
    // -------------------------------------------------------------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryManager.Instance.HandleSlotClick(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    private void OnRightClick()
    {
        if (string.IsNullOrEmpty(itemName) || quantity <= 0)
            return;

        PopupHelpers.ShowRemoveItemPopup(
            InventoryManager.Instance.ItemSlots.ToArray()
        );
    }

    // -------------------------------------------------------------------------
    // DRAG & DROP
    // -------------------------------------------------------------------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (quantity <= 0)
            return;

        BaseItemSO item = InventoryManager.Instance.GetItemSO(itemName);

        // Consumables can ONLY be dragged when inventory is closed
        if (item is ConsumableSO)
        {
            if (InventoryManager.Instance.IsOpen)
                return;

            // Close inventory when dragging a consumable
            InventoryManager.Instance.CloseInventory();
        }
        else
        {
            return;
        }

        // Create drag icon
        dragCanvas = FindFirstObjectByType<Canvas>();

        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(dragCanvas.transform, false);

        Image iconImage = dragIcon.AddComponent<Image>();
        iconImage.sprite = itemSprite;
        iconImage.raycastTarget = false;

        RectTransform rt = dragIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(64, 64);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (dragIcon != null)
            Destroy(dragIcon);

        BaseItemSO item = InventoryManager.Instance.GetItemSO(itemName);
        if (!(item is ConsumableSO))
            return;

        Spot spot = GetClosestSpotToMouse(eventData);
        if (spot != null)
        {
            InventoryManager.Instance.PlaceConsumableOnSpot(this, spot);
        }
    }

    // -------------------------------------------------------------------------
    // SPOT DETECTION WITHOUT COLLIDERS
    // -------------------------------------------------------------------------

    private Spot GetClosestSpotToMouse(PointerEventData eventData)
    {
        SpotController controller = FindFirstObjectByType<SpotController>();
        if (controller == null)
            return null;

        Spot[] allSpots = controller.GetAllSpots();
        if (allSpots == null || allSpots.Length == 0)
            return null;

        Ray ray = Camera.main.ScreenPointToRay(eventData.position);

        Spot closest = null;
        float closestDistance = float.MaxValue;

        foreach (Spot s in allSpots)
        {
            Vector3 spotPos = s.transform.position;

            float distance = Vector3.Cross(ray.direction, spotPos - ray.origin).magnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = s;
            }
        }

        return closest;
    }
}
