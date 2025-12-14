using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Item Properties")]
    [SerializeField] private ItemSO itemData;   // ScriptableObject that defines this item
    [SerializeField] private int quantity = 1;  // How many of this item to give

    private void OnTriggerEnter(Collider other)
    {
        // Only allow pickup when colliding with the Player
        if (other.CompareTag("Player") && itemData != null && InventoryManager.Instance != null)
        {
            int leftOverItems = InventoryManager.Instance.AddItem(
                itemData.itemName,
                quantity,
                itemData.icon,
                itemData.itemDescription
            );

            if (leftOverItems <= 0)
            {
                Destroy(gameObject); // fully picked up
            }
            else
            {
                quantity = leftOverItems; // still some left
            }
        }
    }
}
