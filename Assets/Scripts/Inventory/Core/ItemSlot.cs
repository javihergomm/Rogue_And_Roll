using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/*
 * ItemSlot
 * --------
 * Represents a single inventory slot in the UI.
 * Handles item display, selection, dragging, dropping,
 * and placing consumables onto board Spots or ColorSpots.
 */
public class ItemSlot : MonoBehaviour,
    IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler,
    IPointerEnterHandler, IPointerExitHandler

{
    [SerializeField] private string itemName = "";
    [SerializeField] private int quantity = 0;
    [SerializeField] private Sprite itemSprite;
    [SerializeField] private string itemDescription = "";
    [SerializeField] private Sprite emptySprite;
    private BaseItemSO itemSO;
    public BaseItemSO ItemSO => itemSO;

    public string ItemName => itemName;
    public int Quantity => quantity;
    public Sprite ItemSprite => itemSprite;
    public string ItemDescription => itemDescription;

    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;
    [SerializeField] private GameObject selectedShader;

    public bool ThisItemSelected { get; private set; }

    private CanvasGroup canvasGroup;
    private GameObject dragIcon;
    private Canvas dragCanvas;

    [SerializeField] private RectTransform inventoryPanel;

    private void Awake()
    {
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        RefreshUI();
    }

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

    public void RefreshUI()
    {
        if (itemImage != null)
            itemImage.sprite = quantity > 0 ? itemSprite : emptySprite;

        if (quantityText != null)
            quantityText.text = quantity > 1 ? quantity.ToString() : "";

        if (selectedShader != null)
            selectedShader.SetActive(ThisItemSelected);
    }

    public void SelectSlot()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.DeselectAllSlots();

        ThisItemSelected = true;

        if (selectedShader != null)
            selectedShader.SetActive(true);

        RefreshUI();
    }


    public void DeselectSlot()
    {
        ThisItemSelected = false;
        if (selectedShader != null)
            selectedShader.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        InventoryManager.Instance.HandleSlotClick(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (quantity <= 0)
            return;

        BaseItemSO item = itemSO;

        if (item is PermanentSO)
            return;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
        itemImage.enabled = false;

        if (item is DiceSO)
        {
            if (!InventoryManager.Instance.IsOpen)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
                itemImage.enabled = true;
                return;
            }

            CreateDragIcon();
            eventData.pointerDrag = gameObject;
            return;
        }

        if (item is ConsumableSO consumable)
        {
            if (!consumable.CanBeUsedOnSpot)
            {
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1f;
                itemImage.enabled = true;
                return;
            }

            CreateDragIcon();
            eventData.pointerDrag = gameObject;
            return;
        }

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        itemImage.enabled = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIcon != null)
            dragIcon.transform.position = eventData.position;

        BaseItemSO item = itemSO;

        if (item is not ConsumableSO consumable)
            return;

        if (!consumable.CanBeUsedOnSpot)
            return;

        if (!RectTransformUtility.RectangleContainsScreenPoint(inventoryPanel, eventData.position))
        {
            if (InventoryManager.Instance.IsOpen)
                InventoryManager.Instance.HideInventorySoft();
        }
    }

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

        // Normal Spot
        if (target is Spot spot)
        {
            InventoryManager.Instance.PlaceConsumableOnSpot(this, spot);
            InventoryManager.Instance.CloseInventory();   // FULL CLOSE
            return;
        }

        // ColorSpot with 3D support
        if (target is ColorSpot colorSpot)
        {
            if (consumable.AppearsIn3D)
            {
                InventoryManager.Instance.PlaceConsumableOnColorSpot(this, colorSpot);
                InventoryManager.Instance.CloseInventory();   // FULL CLOSE
            }
            return;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedObject = eventData.pointerDrag;
        if (draggedObject == null)
            return;

        if (!draggedObject.TryGetComponent<ItemSlot>(out var from))
            return;

        InventoryManager.Instance.HandleSlotDrop(from, this);
    }



    private void CreateDragIcon()
    {
        if (dragCanvas == null)
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);

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

    private MonoBehaviour GetClosestSpotToMouse(PointerEventData eventData)
    {
        SpotController controller = Object.FindFirstObjectByType<SpotController>();
        List<MonoBehaviour> all = new();

        if (controller != null)
        {
            Spot[] normalSpots = controller.GetAllSpots();
            if (normalSpots != null && normalSpots.Length > 0)
                all.AddRange(normalSpots);
        }

        ColorSpot[] colorSpots = FindObjectsByType<ColorSpot>(FindObjectsSortMode.None);
        if (colorSpots != null && colorSpots.Length > 0)
            all.AddRange(colorSpots);

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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (quantity <= 0)
            return;

        InventoryDescriptionUI ui = InventoryManager.Instance.DescriptionUI;
        ui.Show(itemName, itemDescription, itemSprite);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryDescriptionUI ui = InventoryManager.Instance.DescriptionUI;
        ui.Clear();
    }

}
