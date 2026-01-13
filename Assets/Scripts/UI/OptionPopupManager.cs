using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/*
 * OptionPopupManager
 * ------------------
 * Manages a single popup panel in the UI Canvas.
 * Supports:
 *   - Choice popups (text + dynamically created buttons)
 *   - Number input popups (text + slider + confirm/cancel)
 *   - Message-only popups (text only, without buttons)
 * Provides helper methods to hide the popup, clear old buttons,
 * and show common popup flows.
 */
public class OptionPopupManager : MonoBehaviour
{
    public static OptionPopupManager Instance { get; private set; }

    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private Button buttonPrefab;

    [SerializeField] private Slider popupSlider;
    [SerializeField] private TextMeshProUGUI sliderLabel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (popupPanel != null) popupPanel.SetActive(false);
        if (popupSlider != null) popupSlider.gameObject.SetActive(false);
        if (sliderLabel != null) sliderLabel.gameObject.SetActive(false);
    }

    /*
     * Shows a generic popup with text and dynamic buttons.
     * Can optionally include a slider for numeric input.
     */
    public void ShowPopup(
        string message,
        Dictionary<string, System.Action> options,
        bool useSlider = false,
        int sliderMax = 0,
        System.Action<int> onConfirmWithNumber = null)
    {
        if (popupPanel == null || popupText == null)
            return;

        if (options == null)
            return;

        popupPanel.SetActive(true);
        popupText.text = message;

        ClearPopupButtons();
        SetupSlider(useSlider, sliderMax);

        foreach (var kvp in options)
        {
            string optionName = kvp.Key;
            System.Action optionAction = kvp.Value;

            Button newButton = CreateOptionButton(optionName);

            newButton.onClick.RemoveAllListeners();
            newButton.onClick.AddListener(() =>
            {
                popupPanel.SetActive(false);

                bool isConfirm = optionName.ToLower().Contains("confirmar");

                if (useSlider && onConfirmWithNumber != null && isConfirm)
                {
                    int value = popupSlider != null ? Mathf.RoundToInt(popupSlider.value) : 1;
                    onConfirmWithNumber(value);
                }
                else
                {
                    optionAction?.Invoke();
                }
            });
        }
    }

    /*
     * Shows a popup with only a message and no buttons.
     */
    public void ShowMessageOnly(string message)
    {
        if (popupPanel == null || popupText == null)
            return;

        popupPanel.SetActive(true);
        popupText.text = message;

        ClearPopupButtons();
        HideSlider();
    }

    /*
     * Hides the popup and clears its content.
     */
    public void HidePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        ClearPopupButtons();
        HideSlider();
    }
    /*
    * Popup to confirm selecting a character.
    */
    public void ShowConfirmCharacterPopup(string characterName, System.Action onConfirm, System.Action onCancel)
    {
        var options = new Dictionary<string, System.Action>
    {
        { "Si", () => { onConfirm?.Invoke(); }},
        { "No", () => { onCancel?.Invoke(); }}
    };

        ShowPopup(
            "¿Estás seguro que quieres elegir " + characterName + " como tu personaje?",
            options
        );
    }

    /*
     * Popup shown when the inventory is full.
     */
    public void ShowInventoryFullPopup(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        var options = new Dictionary<string, System.Action>
        {
            { "Si, reemplazar un objeto", () =>
                {
                    InventoryManager.Instance.PrepareReplace(
                        InventoryManager.Instance.GetItemSO(itemName),
                        quantity
                    );
                    InventoryManager.Instance.OpenInventory();
                }
            },
            { "No reemplazar", () => {} }
        };

        ShowPopup("Inventario lleno. Deseas reemplazar un objeto?", options);
    }

    /*
     * Popup to confirm replacing an item in a slot.
     */
    public void ShowConfirmReplacePopup(ItemSlot slot, System.Action onConfirm)
    {
        string message = "Seguro que quieres reemplazar el objeto '" + slot.itemName + "' en este hueco?";

        var options = new Dictionary<string, System.Action>
        {
            { "Confirmar", () => { onConfirm?.Invoke(); }},
            { "Cancelar", () => {} }
        };

        ShowPopup(message, options);
    }

    /*
     * Popup to remove items from one or more slots.
     */
    public void ShowRemoveItemPopup(ItemSlot[] itemSlots)
    {
        if (itemSlots == null || itemSlots.Length == 0)
            return;

        string itemName = itemSlots[0].itemName;

        var options = new Dictionary<string, System.Action>
        {
            { "Eliminar 1 unidad", () =>
                {
                    InventoryManager.Instance.RemoveItem(itemSlots[0], 1);
                }
            },

            { "Eliminar todo", () =>
                {
                    foreach (var slot in itemSlots)
                    {
                        if (slot.quantity > 0)
                            InventoryManager.Instance.RemoveItem(slot, slot.quantity);
                    }
                }
            },

            { "Eliminar cantidad personalizada", () =>
                {
                    int total = 0;
                    foreach (var slot in itemSlots)
                        total += slot.quantity;

                    ShowNumberSliderPopup(itemSlots, total);
                }
            },

            { "Cancelar", () => {} }
        };

        ShowPopup("Como quieres eliminar " + itemName + "?", options);
    }

    /*
     * Popup with slider to choose a custom amount to remove.
     */
    private void ShowNumberSliderPopup(ItemSlot[] itemSlots, int maxQuantity)
    {
        string itemName = itemSlots[0].itemName;

        var options = new Dictionary<string, System.Action>
        {
            { "Confirmar", () => {} },
            { "Cancelar", () => {} }
        };

        ShowPopup(
            "Selecciona la cantidad de " + itemName + " a eliminar:",
            options,
            useSlider: true,
            sliderMax: maxQuantity,
            onConfirmWithNumber: (amount) =>
            {
                int remaining = amount;

                foreach (var slot in itemSlots)
                {
                    if (remaining <= 0)
                        break;

                    int remove = Mathf.Min(slot.quantity, remaining);
                    InventoryManager.Instance.RemoveItem(slot, remove);
                    remaining -= remove;
                }
            }
        );
    }

    /*
     * Removes old popup buttons.
     */
    private void ClearPopupButtons()
    {
        if (popupPanel == null) return;

        var toDestroy = new List<GameObject>();
        foreach (Transform child in popupPanel.transform)
        {
            if (child.CompareTag("PopupButton"))
                toDestroy.Add(child.gameObject);
        }
        foreach (var go in toDestroy)
            Destroy(go);
    }

    /*
     * Configures the slider if needed.
     */
    private void SetupSlider(bool useSlider, int sliderMax)
    {
        if (popupSlider == null || sliderLabel == null)
        {
            HideSlider();
            return;
        }

        popupSlider.gameObject.SetActive(useSlider);
        sliderLabel.gameObject.SetActive(useSlider);

        if (useSlider)
        {
            popupSlider.minValue = 1;
            popupSlider.maxValue = Mathf.Max(1, sliderMax);
            popupSlider.value = 1;
            sliderLabel.text = "1";

            popupSlider.onValueChanged.RemoveAllListeners();
            popupSlider.onValueChanged.AddListener(val =>
            {
                sliderLabel.text = Mathf.RoundToInt(val).ToString();
            });
        }
    }

    /*
     * Hides the slider.
     */
    private void HideSlider()
    {
        if (popupSlider != null) popupSlider.gameObject.SetActive(false);
        if (sliderLabel != null) sliderLabel.gameObject.SetActive(false);
    }

    /*
     * Creates a new option button.
     */
    private Button CreateOptionButton(string optionName)
    {
        if (buttonPrefab == null || popupPanel == null)
            return null;

        Button newButton = Instantiate(buttonPrefab, popupPanel.transform);
        newButton.tag = "PopupButton";

        TextMeshProUGUI label = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = optionName;

        newButton.gameObject.SetActive(true);

        return newButton;
    }

    /*
     * Popup to confirm exiting the shop.
     */
    public void ShowExitShopPopup(System.Action onConfirm, System.Action onCancel)
    {
        var options = new Dictionary<string, System.Action>
        {
            { "Si", () => { onConfirm?.Invoke(); }},
            { "No", () => { onCancel?.Invoke(); }}
        };

        ShowPopup("Seguro que quieres salir de la tienda?", options);
    }
}
