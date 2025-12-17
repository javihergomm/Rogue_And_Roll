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
 * Provides helper methods to hide the popup, clear old buttons, and show common flows.
 */
public class OptionPopupManager : MonoBehaviour
{
    // Singleton instance so other scripts can call OptionPopupManager.Instance
    public static OptionPopupManager Instance { get; private set; }

    // Core popup references (assign in Inspector)
    [SerializeField] private GameObject popupPanel;       // Root UI panel for the popup
    [SerializeField] private TextMeshProUGUI popupText;   // Text component for the message
    [SerializeField] private Button buttonPrefab;         // Prefab used to create option buttons

    // Optional slider references for numeric inputs
    [SerializeField] private Slider popupSlider;          // Slider used when asking for a quantity
    [SerializeField] private TextMeshProUGUI sliderLabel; // Label to display current slider value

    private void Awake()
    {
        // Ensure a single instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Initialize hidden state
        if (popupPanel != null) popupPanel.SetActive(false);
        if (popupSlider != null) popupSlider.gameObject.SetActive(false);
        if (sliderLabel != null) sliderLabel.gameObject.SetActive(false);
    }

    /*
     * ShowPopup
     * Displays a message and creates one button per entry in 'options'.
     * Optionally shows a slider if 'useSlider' is true.
     * onConfirmWithNumber is invoked with the slider value when a "Confirm" option is clicked.
     */
    public void ShowPopup(
        string message,
        Dictionary<string, System.Action> options,
        bool useSlider = false,
        int sliderMax = 0,
        System.Action<int> onConfirmWithNumber = null)
    {
        if (popupPanel == null || popupText == null)
        {
            Debug.LogWarning("Popup references not set.");
            return;
        }
        if (options == null)
        {
            Debug.LogWarning("Options dictionary is null.");
            return;
        }

        // Activate panel and set message
        popupPanel.SetActive(true);
        popupText.text = message;

        // Remove any previously created buttons
        ClearPopupButtons();

        // Configure slider visibility and behavior
        SetupSlider(useSlider, sliderMax);

        // Create a button for each option
        foreach (var kvp in options)
        {
            string optionName = kvp.Key;
            System.Action optionAction = kvp.Value;

            Button newButton = CreateOptionButton(optionName);

            newButton.onClick.RemoveAllListeners();
            newButton.onClick.AddListener(() =>
            {
                // Hide panel on click
                popupPanel.SetActive(false);

                // If using slider and this is a confirm action, pass the chosen value
                if (useSlider && onConfirmWithNumber != null && optionName.ToLower().Contains("confirm"))
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
     * ShowMessageOnly
     * Displays only a message without creating any buttons.
     * Useful for informational prompts when another system will handle input (e.g., Ouija zones).
     */
    public void ShowMessageOnly(string message)
    {
        if (popupPanel == null || popupText == null)
        {
            Debug.LogWarning("Popup references not set.");
            return;
        }

        popupPanel.SetActive(true);
        popupText.text = message;

        // Ensure there are no buttons or slider visible
        ClearPopupButtons();
        HideSlider();
    }

    /*
     * HidePopup
     * Hides the panel and clears transient UI elements.
     */
    public void HidePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        ClearPopupButtons();
        HideSlider();
    }

    // --------------------------------------------------------------------
    // Convenience wrappers for common flows used elsewhere in your project
    // --------------------------------------------------------------------

    /*
     * ShowInventoryFullPopup
     * Prompts the player to replace an item when inventory is full.
     */
    public void ShowInventoryFullPopup(string itemName, int quantity, Sprite itemSprite, string itemDescription)
    {
        var options = new Dictionary<string, System.Action>
        {
            { "Yes, choose slot to replace", () =>
                {
                    InventoryManager.Instance.PrepareReplace(
                        InventoryManager.Instance.GetItemSO(itemName),
                        quantity
                    );
                    InventoryManager.Instance.OpenInventory();
                    Debug.Log("Select a slot in inventory to replace.");
                }
            },
            { "Do not replace", () => Debug.Log("Keep current inventory.") }
        };

        ShowPopup("Inventory full. Do you want to replace an item?", options);
    }

    /*
     * ShowConfirmReplacePopup
     * Asks for confirmation before replacing an item in a slot.
     */
    public void ShowConfirmReplacePopup(ItemSlot slot, System.Action onConfirm)
    {
        string message = $"Are you sure you want to replace the item '{slot.itemName}' in this slot?";
        var options = new Dictionary<string, System.Action>
        {
            { "Confirm", () => onConfirm?.Invoke() },
            { "Cancel", () => Debug.Log("Replace cancelled.") }
        };

        ShowPopup(message, options);
    }

    /*
     * ShowRemoveItemPopup
     * Stage 1: Asks how to remove an item (one, all, custom amount).
     */
    public void ShowRemoveItemPopup(string itemName, ItemSlot[] itemSlots)
    {
        var options = new Dictionary<string, System.Action>
        {
            { "Remove 1 unit", () => InventoryManager.Instance.RemoveItem(itemName, 1) },
            { "Remove all", () =>
                {
                    int total = 0;
                    foreach (var slot in itemSlots)
                        if (slot.itemName == itemName) total += slot.quantity;
                    InventoryManager.Instance.RemoveItem(itemName, total);
                }
            },
            { "Remove custom amount", () =>
                {
                    int total = 0;
                    foreach (var slot in itemSlots)
                        if (slot.itemName == itemName) total += slot.quantity;
                    ShowNumberSliderPopup(itemName, total);
                }
            },
            { "Cancel", () => Debug.Log("Remove cancelled.") }
        };

        ShowPopup($"How do you want to remove {itemName}?", options);
    }

    /*
     * ShowNumberSliderPopup
     * Stage 2: Shows a slider to choose a quantity and a confirm/cancel pair.
     */
    private void ShowNumberSliderPopup(string itemName, int maxQuantity)
    {
        var options = new Dictionary<string, System.Action>
        {
            { "Confirm", () => {} }, // Action handled via onConfirmWithNumber
            { "Cancel", () => Debug.Log("Custom remove cancelled.") }
        };

        ShowPopup(
            $"Select the amount of {itemName} to remove:",
            options,
            useSlider: true,
            sliderMax: maxQuantity,
            onConfirmWithNumber: (amount) => InventoryManager.Instance.RemoveItem(itemName, amount)
        );
    }

    // --------------------------------------------------------------------
    // Internal helpers
    // --------------------------------------------------------------------

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

    private void HideSlider()
    {
        if (popupSlider != null) popupSlider.gameObject.SetActive(false);
        if (sliderLabel != null) sliderLabel.gameObject.SetActive(false);
    }

        // Creates an option button under the popup panel and tags it for cleanup
    private Button CreateOptionButton(string optionName)
    {
        if (buttonPrefab == null || popupPanel == null)
        {
            Debug.LogWarning("Cannot create option button. Missing references.");
            return null;
        }

        Button newButton = Instantiate(buttonPrefab, popupPanel.transform);
        newButton.tag = "PopupButton";

        TextMeshProUGUI label = newButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null) label.text = optionName;

        // Ensure the button is enabled and visible
        newButton.gameObject.SetActive(true);

        return newButton;
    }

     /*
     * ShowExitShopPopup
     * -----------------
     * Displays a confirmation popup when the player tries to leave the shop.
     * Options:
     *   - "Si" -> confirm exit
     *   - "No" -> cancel exit
     */
    public void ShowExitShopPopup(System.Action onConfirm, System.Action onCancel)
    {
        var options = new Dictionary<string, System.Action>
    {
        { "Si", () => onConfirm?.Invoke() },
        { "No", () => onCancel?.Invoke() }
    };

        ShowPopup("Seguro que quieres salir de la tienda?", options);
    }

}
