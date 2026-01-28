using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/*
 * InventoryPopupUI
 * ----------------
 * Handles:
 * - Inventory full popup
 * - Remove item popup
 */
public class InventoryPopupUI : MonoBehaviour
{
    [Header("Inventory Full Popup")]
    [SerializeField] private GameObject fullPopup;
    [SerializeField] private TMP_Text fullPopupText;
    [SerializeField] private Image fullPopupIcon;

    [Header("Remove Item Popup")]
    [SerializeField] private GameObject removePopup;
    [SerializeField] private TMP_Text removePopupText;

    private Action onConfirmRemove;

    public void ShowInventoryFull(string itemName, Sprite icon)
    {
        fullPopup.SetActive(true);
        fullPopupText.text = $"No hay espacio para {itemName}";
        fullPopupIcon.sprite = icon;
    }

    public void HideInventoryFull()
    {
        fullPopup.SetActive(false);
    }

    public void ShowRemoveItemPopup(string itemName, Action confirm)
    {
        removePopup.SetActive(true);
        removePopupText.text = $"¿Eliminar {itemName}?";
        onConfirmRemove = confirm;
    }

    public void ConfirmRemove()
    {
        onConfirmRemove?.Invoke();
        removePopup.SetActive(false);
    }

    public void CancelRemove()
    {
        removePopup.SetActive(false);
    }
}
