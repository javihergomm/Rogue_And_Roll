using UnityEngine;
using TMPro;
using UnityEngine.UI;

/*
 * InventorySlotUI
 * ----------------
 * Handles the visual representation of a single inventory slot.
 * This class does NOT store item data itself; it only updates the UI
 * based on the information provided by the ItemSlot logic.
 *
 * Responsibilities:
 *   - Display the item icon (or an empty placeholder)
 *   - Display the quantity text (or hide it when quantity is zero)
 *   - Never decides gameplay logic; purely visual
 */
public class InventorySlotUI : MonoBehaviour
{
    // Text element that shows the quantity of the item (e.g. "3")
    [SerializeField] private TMP_Text quantityText;

    // Image element that displays the item's sprite
    [SerializeField] private Image itemImage;

    // Sprite used when the slot is empty (transparent or placeholder)
    [SerializeField] private Sprite emptySprite;

    /*
     * UpdateUI
     * --------
     * Called by ItemSlot whenever the slot's content changes.
     *
     * Parameters:
     *   sprite  -> the icon of the item (null means empty)
     *   quantity -> how many items are in the slot
     *
     * Behavior:
     *   - If sprite is null, show the emptySprite
     *   - If quantity > 0, show the number
     *   - If quantity == 0, hide the number
     */
    public void UpdateUI(Sprite sprite, int quantity)
    {
        // Set the icon (fallback to empty sprite)
        itemImage.sprite = sprite != null ? sprite : emptySprite;

        // Update quantity text
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
