using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * ItemSlot
 * --------
 * Representa un slot de inventario.
 * - Almacena datos del ítem y actualiza su UI.
 * - NO decide si usar, vender o reemplazar: delega en InventoryManager.
 */
public class ItemSlot : MonoBehaviour, IPointerClickHandler
{
    // DATOS DEL ITEM
    public string itemName;
    public int quantity;
    public Sprite itemSprite;
    public bool isFull;
    public string itemDescription;
    public Sprite emptySprite;

    [SerializeField] private int maxNumberOfItems = 10;

    // REFERENCIAS UI
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;

    // UI de descripción
    public Image itemDescriptionImage;
    public TMP_Text itemDescriptionNameText;
    public TMP_Text itemDescriptionText;

    // SELECCIÓN
    [Header("Selection Highlight")]
    public GameObject selectedShader;
    public bool thisItemSelected;

    private void Awake()
    {
        if (quantityText != null)
        {
            quantityText.text = string.Empty;
            quantityText.enabled = false;
        }

        if (selectedShader != null)
            selectedShader.SetActive(false);

        if (itemImage != null)
            itemImage.sprite = emptySprite;
    }

    // Añadir ítems al slot
    public int AddItem(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        Debug.Log($"[AddItem] Sprite recibido: {(itemSprite != null ? itemSprite.name : "null")}");

        if (isFull) return quantity;

        this.itemName = itemName;
        this.itemSprite = itemSprite;
        this.itemDescription = itemDescription;

        itemImage.sprite = itemSprite;
        Debug.Log($"[AddItem] itemImage.sprite asignado: {(itemImage.sprite != null ? itemImage.sprite.name : "null")}");

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


    // Refrescar UI
    public void RefreshUI()
    {
        Debug.Log($"[RefreshUI] Slot: {name} | Qty: {quantity} | itemSprite: {(itemSprite != null ? itemSprite.name : "null")} | itemImage BEFORE: {(itemImage.sprite != null ? itemImage.sprite.name : "null")}");

        if (quantityText != null)
        {
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

        if (itemImage != null)
            itemImage.sprite = quantity > 0 ? itemSprite : emptySprite;

        Debug.Log($"[RefreshUI] Slot: {name} | itemImage AFTER: {(itemImage.sprite != null ? itemImage.sprite.name : "null")}");
    }


    /*
     * OnPointerClick
     * --------------
     * Reenvía los clicks a InventoryManager.
     */
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

    // Seleccionar visualmente este slot
    public void SelectSlot()
    {
        InventoryManager.Instance?.DeselectAllSlots();

        if (selectedShader != null)
            selectedShader.SetActive(true);

        thisItemSelected = true;

        // Actualiza panel de descripción
        if (itemDescriptionNameText != null)
            itemDescriptionNameText.text = itemName;

        if (itemDescriptionText != null)
            itemDescriptionText.text = itemDescription;

        if (itemDescriptionImage != null)
            itemDescriptionImage.sprite = itemSprite ?? emptySprite;

        Debug.Log("[ItemSlot] Slot seleccionado: " + itemName);
    }

    // Deseleccionar visualmente este slot
    public void DeselectSlot()
    {
        if (selectedShader != null)
            selectedShader.SetActive(false);

        thisItemSelected = false;

        Debug.Log("[ItemSlot] Slot deseleccionado: " + itemName);
    }

    // Limpiar slot completamente
    public void ClearSlot()
    {
        quantity = 0;
        isFull = false;
        thisItemSelected = false;

        itemName = string.Empty;
        itemSprite = null;
        itemDescription = string.Empty;

        if (quantityText != null)
        {
            quantityText.text = string.Empty;
            quantityText.enabled = false;
        }

        if (itemImage != null)
            itemImage.sprite = emptySprite;

        if (itemDescriptionNameText != null) itemDescriptionNameText.text = "";
        if (itemDescriptionText != null) itemDescriptionText.text = "";
        if (itemDescriptionImage != null) itemDescriptionImage.sprite = emptySprite;

        if (selectedShader != null) selectedShader.SetActive(false);

        Debug.Log("[ItemSlot] Slot limpiado.");
    }

    private void OnRightClick()
    {
        if (string.IsNullOrEmpty(itemName) || quantity <= 0)
        {
            Debug.Log("[ItemSlot] Click derecho en slot vacío.");
            return;
        }

        OptionPopupManager.Instance.ShowRemoveItemPopup(itemName, InventoryManager.Instance.ItemSlots.ToArray());
    }
}
