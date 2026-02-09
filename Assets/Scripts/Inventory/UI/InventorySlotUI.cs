using UnityEngine;
using TMPro;
using UnityEngine.UI;

/*
 * InventorySlotUI
 * ----------------
 * Displays the item icon and quantity for an inventory slot.
 * Updates the visuals whenever the slot's data changes.
 */
public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Image itemImage;
    [SerializeField] private Sprite emptySprite;

    // Connects this UI element to its corresponding ItemSlot
    public void Initialize(ItemSlot slot)
    {
        // No drag handler needed in the current system
    }

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
