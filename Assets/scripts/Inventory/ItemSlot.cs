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
 * Delegates inventory logic to InventoryManager.
 */
public class ItemSlot : MonoBehaviour,
    IPointerClickHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    // Item data stored in this slot
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;

    [SerializeField] private int maxNumberOfItems = 10;

    // UI elements that display the slot contents
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;

    // UI elements used to show item details elsewhere
    public Image itemDescriptionImage;
    public TMP_Text itemDescriptionNameText;
    public TMP_Text itemDescriptionText;

    // Visual highlight for selection
    [Header("Selection Highlight")]
    public GameObject selectedShader;
    public bool thisItemSelected;

    // Drag and drop helpers
    private CanvasGroup canvasGroup;
    private GameObject dragIcon;
    private Canvas dragCanvas;

    private void Awake()
    {
        // Add CanvasGroup for transparency and raycast control
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

    // Inserts item data into this slot and updates its visuals
    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        if (isFull) return quantity;

        this.itemName = itemName;
        this.itemSprite = itemSprite;
        this.itemDescription = itemDescription;

        itemImage.sprite = itemSprite;

        this.quantity += quantity;

        if (this.quantity >= maxNumberOfItems)
        {
            int extra = this.quantity - maxNumberOfItems;
            this.quantity = maxNumberOfItems;
            isFull = true;
            RefreshUI();
            return extra;
        }

        RefreshUI();
        return 0;
    }

    // Updates the slot visuals to match its current data
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

    // Handles left and right click interactions
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryManager.Instance.OnSlotClicked(this);
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    // Highlights this slot and updates the description panel
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

    // Removes highlight from this slot
    public void DeselectSlot()
    {
        if (selectedShader != null)
            selectedShader.SetActive(false);

        thisItemSelected = false;
    }

    // Resets this slot to an empty state
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

    // Opens the remove-item popup
    private void OnRightClick()
    {
        if (string.IsNullOrEmpty(itemName) || quantity <= 0)
            return;

        OptionPopupManager.Instance.ShowRemoveItemPopup(
            itemName,
            InventoryManager.Instance.ItemSlots.ToArray()
        );
    }

    // -------------------------------------------------------------------------
    // DRAG AND DROP
    // -------------------------------------------------------------------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Cannot drag empty slots
        if (quantity <= 0)
            return;

        // Only dice can be dragged
        BaseItemSO item = InventoryManager.Instance.GetItemSO(itemName);
        if (!(item is DiceSO))
            return;

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

        // If the item was not a dice, no drag happened
        BaseItemSO item = InventoryManager.Instance.GetItemSO(itemName);
        if (!(item is DiceSO))
            return;

        // Detect drop target
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var hit in results)
        {
            ItemSlot targetSlot = hit.gameObject.GetComponent<ItemSlot>();
            if (targetSlot != null)
            {
                InventoryManager.Instance.HandleSlotDrop(this, targetSlot);
                return;
            }
        }
    }
}
