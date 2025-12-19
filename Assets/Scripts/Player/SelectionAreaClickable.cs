using UnityEngine;

/*
 * SelectionAreaClickable
 * ----------------------
 * Attached to each empty area (character zones).
 * Responsibilities:
 * - Detect mouse click on the area
 * - Call CupSelector.ApplySelection with area info
 */
public class SelectionAreaClickable : MonoBehaviour
{
    [Tooltip("Name of the character/area.")]
    public string areaName;

    [Tooltip("Color to apply to the cup when clicked.")]
    public Color areaColor = Color.white;

    [Tooltip("Reference to the cup selector.")]
    [SerializeField] private CupSelector cupSelector;

    private void OnMouseDown()
    {
        Debug.Log("[SelectionAreaClickable] Click detected on area: " + areaName);

        if (cupSelector == null)
        {
            Debug.LogWarning("[SelectionAreaClickable] CupSelector reference is missing!");
            return;
        }

        Debug.Log("[SelectionAreaClickable] CupSelector reference OK");

        if (cupSelector.SelectionDone)
        {
            Debug.Log("[SelectionAreaClickable] Selection already done.");
            return;
        }

        if (cupSelector.ShopExitManager != null)
            Debug.Log("[SelectionAreaClickable] IsInShop = " + cupSelector.ShopExitManager.IsInShop());

        if (cupSelector.ShopExitManager != null && cupSelector.ShopExitManager.IsInShop())
        {
            Debug.Log("[SelectionAreaClickable] Still in shop, cannot select.");
            return;
        }

        Debug.Log("[SelectionAreaClickable] Calling ApplySelection...");
        cupSelector.ApplySelection(areaName, areaColor, transform);
    }

}
