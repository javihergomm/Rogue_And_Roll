using UnityEngine;
using TMPro;
using UnityEngine.UI;

/*
 * InventorySlotUI
 * ----------------
 * Pure UI component for a slot.
 * ItemSlot calls this to update visuals.
 */
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Sprite emptySprite;

    public void UpdateUI(Sprite sprite, int quantity)
    {
        itemImage.sprite = sprite != null ? sprite : emptySprite;

        if (quantityText != null)
        {
            if (quantity > 0)
            {
                quantityText.text = quantity.ToString();
                quantityText.enabled = true;
            }
            else
            {
                quantityText.text = "";
                quantityText.enabled = false;
            }
        }
    }
}

