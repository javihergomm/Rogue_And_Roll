using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/*
 * OptionPopupManager
 * Handles showing a single popup panel for:
 *   - Normal choice popups (text + buttons)
 *   - Number input popups (slider capped at max quantity)
 * Reuses the same PopupPanel prefab in your Canvas.
 * Includes fallback logic if references are not set.
 */
public class OptionPopupManager : MonoBehaviour
{
    public static OptionPopupManager Instance { get; private set; }

    // Popup references
    [SerializeField] private GameObject popupPanel;          // The panel itself
    [SerializeField] private TextMeshProUGUI popupText;      // Message text
    [SerializeField] private Button buttonPrefab;            // Prefab for option buttons

    // Slider references for quantity selection
    [SerializeField] private Slider popupSlider;             // Slider for quantity
    [SerializeField] private TextMeshProUGUI sliderLabel;    // Shows current slider value

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Hide panel and slider at start
        if (popupPanel != null) popupPanel.SetActive(false);
        if (popupSlider != null) popupSlider.gameObject.SetActive(false);
        if (sliderLabel != null) sliderLabel.gameObject.SetActive(false);
    }

    /*
     * Show a popup with message, options, and optional slider.
     * Falls back to direct execution if references are missing.
     */
    public void ShowPopup(
        string message,
        Dictionary<string, System.Action> options,
        bool useSlider = false,
        int sliderMax = 0,
        System.Action<int> onConfirmWithNumber = null)
    {
        // Fallback if references are missing
        if (popupPanel == null || popupText == null || buttonPrefab == null)
        {
            Debug.LogWarning("Popup references not set. Falling back to direct execution.");

            if (useSlider && onConfirmWithNumber != null)
            {
                // Default to 1 if slider not available
                onConfirmWithNumber(1);
            }
            else
            {
                // Execute the first option by default
                foreach (var kvp in options)
                {
                    kvp.Value?.Invoke();
                    break;
                }
            }
            return;
        }

        // Normal popup flow
        popupPanel.SetActive(true);
        popupText.text = message;

        // Clear old buttons
        foreach (Transform child in popupPanel.transform)
        {
            if (child.CompareTag("PopupButton"))
                Destroy(child.gameObject);
        }

        // Slider logic
        if (popupSlider != null && sliderLabel != null)
        {
            popupSlider.gameObject.SetActive(useSlider);
            sliderLabel.gameObject.SetActive(useSlider);

            if (useSlider)
            {
                popupSlider.minValue = 1;
                popupSlider.maxValue = sliderMax;
                popupSlider.value = 1;
                sliderLabel.text = "1";

                popupSlider.onValueChanged.RemoveAllListeners();
                popupSlider.onValueChanged.AddListener(val =>
                {
                    sliderLabel.text = Mathf.RoundToInt(val).ToString();
                });
            }
        }

        // Create new buttons
        foreach (var kvp in options)
        {
            string optionName = kvp.Key;
            System.Action optionAction = kvp.Value;

            Button newButton = Instantiate(buttonPrefab, popupPanel.transform);
            newButton.tag = "PopupButton";
            newButton.GetComponentInChildren<TextMeshProUGUI>().text = optionName;

            newButton.onClick.RemoveAllListeners();
            newButton.onClick.AddListener(() =>
            {
                popupPanel.SetActive(false);

                if (useSlider && onConfirmWithNumber != null && optionName.ToLower().Contains("confirm"))
                {
                    int value = Mathf.RoundToInt(popupSlider.value);
                    onConfirmWithNumber(value);
                }
                else
                {
                    optionAction?.Invoke();
                }
            });
        }
    }

    /* ------------------ Specific convenience wrappers ------------------ */

    // Inventory full popup
    public void ShowInventoryFullPopup(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        var options = new Dictionary<string, System.Action>
        {
            { "Sí, elegir objeto a reemplazar", () => {
                InventoryManager.Instance.PrepareReplace(itemName, quantity, itemSprite, itemDescription);
                InventoryManager.Instance.OpenInventory();
                Debug.Log("Selecciona un slot en el inventario para reemplazar");
            }},
            { "No reemplazar", () => Debug.Log("Mantener inventario actual") }
        };

        ShowPopup("Inventario lleno. ¿Quieres reemplazar un objeto?", options);
    }

    // Confirmation before replacing a slot
    public void ShowConfirmReplacePopup(ItemSlot slot, System.Action onConfirm)
    {
        string message = $"¿Seguro que quieres reemplazar el objeto '{slot.itemName}' en este slot?";
        var options = new Dictionary<string, System.Action>
        {
            { "Confirmar", () => onConfirm?.Invoke() },
            { "Cancelar", () => Debug.Log("Reemplazo cancelado") }
        };

        ShowPopup(message, options);
    }

    // Stage 1: Remove item popup with options
    public void ShowRemoveItemPopup(string itemName, ItemSlot[] itemSlots)
    {
        var options = new Dictionary<string, System.Action>
        {
            { "Eliminar 1 unidad", () => InventoryManager.Instance.RemoveItem(itemName, 1) },
            { "Eliminar todas", () => {
                int total = 0;
                foreach (var slot in itemSlots)
                    if (slot.itemName == itemName) total += slot.quantity;
                InventoryManager.Instance.RemoveItem(itemName, total);
            }},
            { "Eliminar cantidad personalizada", () => {
                int total = 0;
                foreach (var slot in itemSlots)
                    if (slot.itemName == itemName) total += slot.quantity;
                ShowNumberSliderPopup(itemName, total);
            }},
            { "Cancelar", () => Debug.Log("Eliminación cancelada") }
        };

        ShowPopup($"¿Cómo quieres eliminar {itemName}?", options);
    }

    // Stage 2: slider popup
    private void ShowNumberSliderPopup(string itemName, int maxQuantity)
    {
        var options = new Dictionary<string, System.Action>
        {
            { "Confirmar", () => {} }, // handled by slider
            { "Cancelar", () => Debug.Log("Eliminación personalizada cancelada") }
        };

        ShowPopup(
            $"Selecciona la cantidad de {itemName} a eliminar:",
            options,
            useSlider: true,
            sliderMax: maxQuantity,
            onConfirmWithNumber: (amount) => InventoryManager.Instance.RemoveItem(itemName, amount)
        );
    }
}
