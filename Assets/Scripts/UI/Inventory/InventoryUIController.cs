using UnityEngine;

/*
 * InventoryUIController
 * ---------------------
 * Controls the inventory menu UI.
 * - Opens/closes the menu
 * - Refreshes all slot UIs
 * - Listens to InventoryManager events
 */
public class InventoryUIController : MonoBehaviour
{
    [SerializeField] private GameObject inventoryMenu;

    private void OnEnable()
    {
        InventoryManager.Instance.OnInventoryChanged += RefreshAllSlots;
        InventoryManager.Instance.OnActiveDiceChanged += RefreshAllSlots;
    }

    private void OnDisable()
    {
        InventoryManager.Instance.OnInventoryChanged -= RefreshAllSlots;
        InventoryManager.Instance.OnActiveDiceChanged -= RefreshAllSlots;
    }

    public void ToggleInventory()
    {
        InventoryManager.Instance.ToggleInventory();
    }

    public void RefreshAllSlots()
    {
        foreach (var slot in InventoryManager.Instance.AllSlots)
            slot.RefreshUI();
    }
}

