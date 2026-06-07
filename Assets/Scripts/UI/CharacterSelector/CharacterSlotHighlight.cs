using UnityEngine;

/*
 * Handles the highlight state of a character slot.
 * This is used by the selection system to show when a slot is active.
 */
public class CharacterSlotHighlight : MonoBehaviour
{
    [SerializeField] private GameObject selectedShader;

    // Enables the highlight
    public void Select()
    {
        if (selectedShader != null)
            selectedShader.SetActive(true);
    }

    // Disables the highlight
    public void Deselect()
    {
        if (selectedShader != null)
            selectedShader.SetActive(false);
    }
}
