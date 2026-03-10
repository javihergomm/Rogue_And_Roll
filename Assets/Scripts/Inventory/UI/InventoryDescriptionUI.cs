using UnityEngine;
using TMPro;
using UnityEngine.UI;

/*
 * InventoryDescriptionUI
 * ----------------------
 * Displays the selected item's:
 * - name
 * - description
 * - icon
 */
public class InventoryDescriptionUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite emptySprite;

    public void Show(string name, string desc, Sprite icon)
    {
        nameText.text = name;
        descriptionText.text = desc;
        iconImage.sprite = icon != null ? icon : emptySprite;
    }

    public void Clear()
    {
        nameText.text = "";
        descriptionText.text = "";
        iconImage.sprite = emptySprite;
    }
}
