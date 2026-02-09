using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * ItemSlot
 * --------
 * Manages a single inventory slot:
 *  - Stores item data and updates its UI
 *  - Allows selecting items
 *  - Allows dragging dice inside the inventory
 *  - Allows dragging consumables out of the inventory panel to place them on board Spots
 *  - Allows receiving dropped items from other slots
 */
public class ItemSlot : MonoBehaviour,
    IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    // -------------------------------------------------------------------------
    // DATA
    // -------------------------------------------------------------------------

    [SerializeField] private string itemName = "";
    [SerializeField] private int quantity = 0;
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private string itemDescription = "";
    [SerializeField] private Sprite emptySprite;

    public string ItemName => itemName;
    public int Quantity => quantity;
    public Sprite ItemSprite => itemSprite;
    public string ItemDescription => itemDescription;

    // -------------------------------------------------------------------------
    // UI
    // -------------------------------------------------------------------------

    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject selectedShader;

    public bool thisItemSelected { get; private set; }

    // -------------------------------------------------------------------------
    // DRAG & DROP
    // -------------------------------------------------------------------------

    private CanvasGroup canvasGroup;
    private GameObject dragIcon;
    private Canvas rootCanvas;

    [SerializeField] private RectTransform inventoryPanel;

    private void Awake()
    {
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        RefreshUI();
    }

    // -------------------------------------------------------------------------
    // ITEM MANAGEMENT
    // -------------------------------------------------------------------------

    public int AddItem(string name, int qty, Sprite sprite, string description)
    {
        itemName = name;
        itemSprite = sprite;
        itemDescription = description;

        quantity += qty;
        RefreshUI();
        return 0;
    }

    public void ClearSlot()
    {
        itemName = "";
        itemSprite = null;
        itemDescription = "";
        quantity = 0;
        thisItemSelected = false;

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (itemImage != null)
            itemImage.sprite = quantity > 0 ? itemSprite : emptySprite;

        if (quantityText != null)
            quantityText.text = quantity > 1 ? quantity.ToString() : "";

        if (selectedShader != null)
            selectedShader.SetActive(thisItemSelected);
    }

    // -------------------------------------------------------------------------
    // SELECTION
    // -------------------------------------------------------------------------

    public void SelectSlot()
    {
        InventoryManager.Instance?.DeselectAllSlots();
        thisItemSelected = true;
        selectedShader?.SetActive(true);
        RefreshUI();
    }

    public void DeselectSlot()
    {
        thisItemSelected = false;
        selectedShader?.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // CLICK HANDLING
    // -------------------------------------------------------------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryManager.Instance.HandleSlotClick(this);
    }

    // -------------------------------------------------------------------------
    // DRAG & DROP
    // -------------------------------------------------------------------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (quantity <= 0)
            return;

        BaseItemSO item = InventoryManager.Instance.GetItemSO(itemName);

        if (item is PermanentSO)
            return;

        if (item is DiceSO)
        {
            if (!InventoryManager.Instance.IsOpen)
                return;

            CreateDragIcon();
            return;
        }

        if (item is ConsumableSO)
        {
            CreateDragIcon();
            return;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            dragIcon.transform.position = eventData.position;

        BaseItemSO item = InventoryManager.Instance.GetItemSO(itemName);

        if (!(item is ConsumableSO))
            return;

        if (!RectTransformUtility.RectangleContainsScreenPoint(inventoryPanel, eventData.position))
        {
            if (InventoryManager.Instance.IsOpen)
                InventoryManager.Instance.CloseInventory();
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (dragIcon != null)
            DestroyImmediate(dragIcon);

        BaseItemSO item = InventoryManager.Instance.GetItemSO(itemName);

        if (item is DiceSO)
            return;

        if (item is ConsumableSO)
        {
            Spot spot = GetClosestSpotToMouse(eventData);
            if (spot != null)
                InventoryManager.Instance.PlaceConsumableOnSpot(this, spot);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        ItemSlot from = eventData.pointerDrag?.GetComponent<ItemSlot>();
        if (from == null)
            return;

        InventoryManager.Instance.HandleSlotDrop(from, this);
    }

    // -------------------------------------------------------------------------
    // DRAG ICON CREATION
    // -------------------------------------------------------------------------

    private void CreateDragIcon()
    {
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(rootCanvas.transform, false);

        Image iconImage = dragIcon.AddComponent<Image>();
        iconImage.sprite = itemSprite;
        iconImage.raycastTarget = false;

        RectTransform rt = dragIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(64, 64);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    // -------------------------------------------------------------------------
    // SPOT DETECTION
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
            float distance = Vector3.Cross(ray.direction, s.transform.position - ray.origin).magnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = s;
            }
        }

        return closest;
    }
}
