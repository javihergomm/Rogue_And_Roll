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
                Debug.Log("Popup button clicked: " + optionName);

                popupPanel.SetActive(false);

                if (useSlider && onConfirmWithNumber != null && optionName.ToLower().Contains("confirm"))
                {
                    int value = popupSlider != null ? Mathf.RoundToInt(popupSlider.value) : 1;
                    Debug.Log("Popup confirm with slider value: " + value);
                    onConfirmWithNumber(value);
                }
                else
                {
                    Debug.Log("Invoking action for option: " + optionName);
                    optionAction?.Invoke();
                }
            });
        }
    }

    public void ShowMessageOnly(string message)
    {
        if (popupPanel == null || popupText == null)
        {
            Debug.LogWarning("Popup references not set.");
            return;
        }

        popupPanel.SetActive(true);
        popupText.text = message;

        ClearPopupButtons();
        HideSlider();
    }

    public void HidePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        ClearPopupButtons();
        HideSlider();
    }

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

    public void ShowConfirmReplacePopup(ItemSlot slot, System.Action onConfirm)
    {
        string message = $"Are you sure you want to replace the item '{slot.itemName}' in this slot?";
        var options = new Dictionary<string, System.Action>
        {
            { "Confirm", () => {
                Debug.Log("ConfirmReplacePopup: Confirm pressed");
                onConfirm?.Invoke();
            }},
            { "Cancel", () => Debug.Log("Replace cancelled.") }
        };

        ShowPopup(message, options);
    }

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

    private void ShowNumberSliderPopup(string itemName, int maxQuantity)
    {
        var options = new Dictionary<string, System.Action>
        {
            { "Confirm", () => {} },
            { "Cancel", () => Debug.Log("Custom remove cancelled.") }
        };

        ShowPopup(
            $"Select the amount of {itemName} to remove:",
            options,
            useSlider: true,
            sliderMax: maxQuantity,
            onConfirmWithNumber: (amount) => {
                Debug.Log("NumberSliderPopup: Confirm pressed with amount " + amount);
                InventoryManager.Instance.RemoveItem(itemName, amount);
            }
        );
    }

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

        newButton.gameObject.SetActive(true);

        Debug.Log("Created popup button: " + optionName);

        return newButton;
    }

    public void ShowExitShopPopup(System.Action onConfirm, System.Action onCancel)
    {
        var options = new Dictionary<string, System.Action>
        {
            { "Si", () => {
                Debug.Log("ExitShopPopup: Confirm pressed");
                onConfirm?.Invoke();
            }},
            { "No", () => {
                Debug.Log("ExitShopPopup: Cancel pressed");
                onCancel?.Invoke();
            }}
        };

        ShowPopup("Seguro que quieres salir de la tienda?", options);
    }
}
