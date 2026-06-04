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
 *   - Timed messages
 *   - Hiding and clearing UI
 *
 * Contains no gameplay logic.
 */
public class OptionPopupManager : MonoBehaviour
{
    public static OptionPopupManager Instance { get; private set; }

    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private Button buttonPrefab;

    private readonly List<Button> activeButtons = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // PUBLIC API
    // -------------------------------------------------------------------------

    /*
     * Shows a popup with a message and a list of button options.
     */
    public void ShowPopup(string message, List<PopupOption> options)
    {
        if (popupPanel == null || popupText == null)
            return;

        popupPanel.SetActive(true);
        popupText.text = message;

        ClearButtons();

        foreach (var opt in options)
            CreateButton(opt);
    }

    /*
     * Shows a popup with only a message and no buttons.
     */
    public void ShowMessage(string message)
    {
        if (popupPanel != null)
            popupPanel.SetActive(true);

        if (popupText != null)
            popupText.text = message;

        ClearButtons();
    }

    /*
     * Hides the popup and clears all UI elements.
     */
    public void HidePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);

        ClearButtons();
    }

    public bool IsPopupOpen => popupPanel != null && popupPanel.activeSelf;

    // -------------------------------------------------------------------------
    // INTERNAL UI
    // -------------------------------------------------------------------------

    /*
     * Creates a button for a popup option.
     */
    private void CreateButton(PopupOption option)
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
            if (popupPanel != null)
                popupPanel.SetActive(false);

            option.Callback?.Invoke();
        });
    }

    /*
     * Removes all active buttons from the popup.
     */
    private void ClearButtons()
    {
        foreach (var btn in activeButtons)
        {
            if (btn != null)
                Destroy(btn.gameObject);
        }

        activeButtons.Clear();
    }

    // -------------------------------------------------------------------------
    // TIMED MESSAGE
    // -------------------------------------------------------------------------

    /*
     * Shows a popup message for a limited duration.
     */
    public void ShowTimedMessage(string message, float duration)
    {
        if (popupPanel != null)
            popupPanel.SetActive(true);

        if (popupText != null)
            popupText.text = message;

        ClearButtons();

        StartCoroutine(AutoHide(duration));
    }

    private System.Collections.IEnumerator AutoHide(float delay)
    {
        yield return new WaitForSeconds(delay);
        HidePopup();
    }
}
