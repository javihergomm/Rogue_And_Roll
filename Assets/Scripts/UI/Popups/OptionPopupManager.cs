using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/*
 * OptionPopupManager
 * ------------------
 * Pure UI controller for popup panels.
 * Handles:
 *   - Showing text
 *   - Creating buttons
 *   - Optional slider
 *   - Hiding/clearing UI
 *
 * Does NOT contain game logic.
 */
public class OptionPopupManager : MonoBehaviour
{
    public static OptionPopupManager Instance { get; private set; }

    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private Button buttonPrefab;

    [SerializeField] private Slider popupSlider;
    [SerializeField] private TextMeshProUGUI sliderLabel;

    private readonly List<Button> activeButtons = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        popupPanel?.SetActive(false);
        popupSlider?.gameObject.SetActive(false);
        sliderLabel?.gameObject.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // PUBLIC API
    // -------------------------------------------------------------------------

    public void ShowPopup(
        string message,
        List<PopupOption> options,
        bool useSlider = false,
        int sliderMax = 0,
        System.Action<int> onConfirmWithNumber = null)
    {
        if (popupPanel == null || popupText == null)
            return;

        popupPanel.SetActive(true);
        popupText.text = message;

        ClearButtons();
        SetupSlider(useSlider, sliderMax);

        foreach (var opt in options)
            CreateButton(opt, useSlider, onConfirmWithNumber);
    }

    public void ShowMessage(string message)
    {
        popupPanel?.SetActive(true);
        popupText.text = message;

        ClearButtons();
        HideSlider();
    }

    public void HidePopup()
    {
        popupPanel?.SetActive(false);
        ClearButtons();
        HideSlider();
    }

    public bool IsPopupOpen => popupPanel != null && popupPanel.activeSelf;

    // -------------------------------------------------------------------------
    // INTERNAL UI
    // -------------------------------------------------------------------------

    private void CreateButton(
        PopupOption option,
        bool useSlider,
        System.Action<int> onConfirmWithNumber)
    {
        if (buttonPrefab == null || popupPanel == null)
            return;

        Button btn = Instantiate(buttonPrefab, popupPanel.transform);
        activeButtons.Add(btn);

        TextMeshProUGUI label = btn.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = option.Label;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            popupPanel.SetActive(false);

            if (useSlider && onConfirmWithNumber != null && option.IsConfirm)
            {
                int value = popupSlider != null ? Mathf.RoundToInt(popupSlider.value) : 1;
                onConfirmWithNumber(value);
            }
            else
            {
                option.Callback?.Invoke();
            }
        });
    }

    private void ClearButtons()
    {
        foreach (var btn in activeButtons)
            if (btn != null)
                Destroy(btn.gameObject);

        activeButtons.Clear();
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

        if (!useSlider)
            return;

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

    private void HideSlider()
    {
        popupSlider?.gameObject.SetActive(false);
        sliderLabel?.gameObject.SetActive(false);
    }
}
