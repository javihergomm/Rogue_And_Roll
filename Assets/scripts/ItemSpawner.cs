using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab; // Prefab to spawn (your D6 prefab)

    private void Start()
    {
        if (itemPrefab != null)
        {
            // Spawn the prefab at this empty's position
            Instantiate(itemPrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.LogWarning("No itemPrefab assigned to ItemSpawner.");
        }
    }
}
