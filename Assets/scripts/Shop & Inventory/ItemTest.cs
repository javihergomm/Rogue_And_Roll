using UnityEngine;

public class ItemTest : MonoBehaviour
{
    [SerializeField] private ItemSO itemData;   // ScriptableObject that defines this item
    [SerializeField] private int quantity = 1;  // How many of this item to give

    private GameObject spawnedModel;

    private void Start()
    {
        Debug.Log("ItemTest.Start() called on " + gameObject.name);

    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter fired on " + gameObject.name + " with collider: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Collider belongs to Player.");

            if (itemData != null && InventoryManager.Instance != null)
            {
                Debug.Log("Player touched item: " + itemData.itemName +
                          " | Quantity: " + quantity);

                int leftOverItems = InventoryManager.Instance.AddItem(
                    itemData.itemName,
                    quantity,
                    itemData.icon,
                    itemData.itemDescription
                );

                Debug.Log("InventoryManager.AddItem returned leftover = " + leftOverItems);

                if (leftOverItems <= 0)
                {
                    Debug.Log("Item fully consumed, destroying " + gameObject.name);
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("Item partially consumed, updating quantity to " + leftOverItems);
                    quantity = leftOverItems;
                }
            }
            else
            {
                Debug.LogWarning("ItemData or InventoryManager.Instance is null on " + gameObject.name);
            }
        }
        else
        {
            Debug.Log("OnTriggerEnter ignored: collider is not Player (tag=" + other.tag + ")");
        }
    }
}
