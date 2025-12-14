using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
 * ItemSlot
 * --------
 * Represents a single inventory slot.
 * It stores item data and updates its UI.
 * IMPORTANT:
 * - This class does NOT decide whether to use, sell, or replace items.
 * - All click logic is forwarded to InventoryManager.
 * - InventoryManager decides what to do depending on the current mode.
 */
public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    //====== ITEM DATA ======//
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;

    [SerializeField] private int maxNumberOfItems = 10;

    //====== UI REFERENCES ======//
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;

    //====== ITEM DESCRIPTION UI ======//
    public Image itemDescriptionImage;
    public TMP_Text itemDescriptionNameText;
    public TMP_Text itemDescriptionText;

    //====== SELECTION ======//
    public GameObject selectedShader;
    public bool thisItemSelected;

    private void Awake()
    {
        if (quantityText != null)
        {
            quantityText.text = string.Empty;
            quantityText.enabled = false;
        }

        selectedShader?.SetActive(false);
    }

    // Add items to this slot
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
            int extraItems = this.quantity - maxNumberOfItems;
            this.quantity = maxNumberOfItems;
            isFull = true;
            RefreshUI();
            return extraItems;
        }

        RefreshUI();
        return 0;
    }

    // Refresh UI
    public void RefreshUI()
    {
        if (quantityText != null)
        {
            quantityText.ForceMeshUpdate();

            if (quantity > 0)
            {
                quantityText.text = quantity.ToString();
                quantityText.enabled = true;
            }
            else
            {
                quantityText.text = string.Empty;
                quantityText.enabled = false;
            }
        }
    }

    /*
     * OnPointerClick
     * --------------
     * Forwards all clicks to InventoryManager.
     * InventoryManager decides:
     * - Use item (normal mode)
     * - Replace item (inventory full mode)
     * - Sell item (sell pedestal mode)
     */
    public void OnPointerClick(PointerEventData eventData)
    {
        // Left click -> forward to InventoryManager
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            InventoryManager.Instance.OnSlotClicked(this);
        }
        // Right click -> delete popup (SPANISH)
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick();
        }
    }

    // Select this slot visually
    public void SelectSlot()
    {
        InventoryManager.Instance?.DeselectAllSlots();

        selectedShader?.SetActive(true);
        thisItemSelected = true;

        if (itemDescriptionNameText != null)
            itemDescriptionNameText.text = itemName;

        if (itemDescriptionText != null)
            itemDescriptionText.text = itemDescription;

        if (itemDescriptionImage != null)
            itemDescriptionImage.sprite = itemSprite ?? emptySprite;
    }

    // Clear slot completely
    public void ClearSlot()
    {
        quantity = 0;
        isFull = false;
        thisItemSelected = false;

        itemName = string.Empty;
        itemSprite = null;
        itemDescription = string.Empty;

        if (quantityText != null) quantityText.enabled = false;
        if (itemImage != null) itemImage.sprite = emptySprite;

        if (itemDescriptionNameText != null) itemDescriptionNameText.text = "";
        if (itemDescriptionText != null) itemDescriptionText.text = "";
        if (itemDescriptionImage != null) itemDescriptionImage.sprite = emptySprite;

        if (selectedShader != null) selectedShader.SetActive(false);
    }

    // Right click -> delete popup (SPANISH)
    private void OnRightClick()
    {
        if (string.IsNullOrEmpty(itemName) || quantity <= 0)
        {
            Debug.Log("Has hecho clic derecho en un hueco vacio.");
            return;
        }

        OptionPopupManager.Instance.ShowRemoveItemPopup(itemName, InventoryManager.Instance.ItemSlots);
    }
}
