using UnityEngine;

public class Loot : MonoBehaviour
{
    public ItemSO itemSO;  // Assigned at runtime

    private GameObject spawnedModel;

    private void Start()
    {
        if (itemSO != null)
        {
            SpawnModel();
            this.name = itemSO.itemName;
        }
    }

    public void SpawnModel()
    {
        if (itemSO != null && itemSO.prefab3D != null)
        {
            // Destroy previous model if it exists
            if (spawnedModel != null)
                Destroy(spawnedModel);

            spawnedModel = Instantiate(itemSO.prefab3D, transform);
            spawnedModel.transform.localPosition = Vector3.zero;
            spawnedModel.transform.localRotation = Quaternion.identity;

            // Optional: make sure it’s scaled to something visible
            spawnedModel.transform.localScale = Vector3.one * 1.5f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

    }
}
