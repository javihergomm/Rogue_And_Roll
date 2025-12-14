using UnityEngine;
using System.Collections.Generic;

public class ShopPedestalRandomizer : MonoBehaviour
{
    [Header("Possible items for this pedestal")]
    [SerializeField] private ItemSO[] possibleItems;

    [Header("Visuals")]
    [SerializeField] private Transform displayPoint;
    [SerializeField] private float displayYOffset = 0.1f;

    private ItemSO chosenItem;
    private GameObject spawnedModel;

    // Prevents multiple spawns in the same shop visit
    private bool hasGeneratedThisVisit = false;

    private void Start()
    {
        // Optional: only generate on Start if the shop is loaded fresh
        RefreshItem();
        hasGeneratedThisVisit = true;
    }

    /*
     * Called by the shop when the player enters the shop area.
     * This ensures the pedestal only generates once per visit.
     */
    public void GenerateIfNeeded()
    {
        if (!hasGeneratedThisVisit)
        {
            RefreshItem();
            hasGeneratedThisVisit = true;
        }
    }

    /*
     * Called by the shop when the player leaves the shop.
     * This resets the pedestal so it can generate again next time.
     */
    public void ResetForNextVisit()
    {
        hasGeneratedThisVisit = false;
    }

    private void RefreshItem()
    {
        if (possibleItems == null || possibleItems.Length == 0)
        {
            Debug.LogWarning("No possible items assigned to ShopPedestalRandomizer!");
            return;
        }

        if (spawnedModel != null)
            Destroy(spawnedModel);

        int index = Random.Range(0, possibleItems.Length);
        chosenItem = possibleItems[index];

        if (chosenItem.prefab3D != null && displayPoint != null)
        {
            Collider pedestalCollider = GetComponentInChildren<Collider>();
            Vector3 worldSpawnPos;

            if (pedestalCollider != null)
            {
                Bounds bounds = pedestalCollider.bounds;
                worldSpawnPos = new Vector3(bounds.center.x, bounds.max.y + displayYOffset, bounds.center.z);
            }
            else
            {
                worldSpawnPos = transform.position + Vector3.up * (1f + displayYOffset);
            }

            Vector3 localSpawnPos = displayPoint.InverseTransformPoint(worldSpawnPos);

            spawnedModel = Instantiate(chosenItem.prefab3D, displayPoint);
            spawnedModel.transform.localPosition = localSpawnPos;
            spawnedModel.transform.localRotation = Quaternion.identity;

            foreach (var rb in spawnedModel.GetComponentsInChildren<Rigidbody>())
                Destroy(rb);

            foreach (var col in spawnedModel.GetComponentsInChildren<Collider>())
                Destroy(col);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (OptionPopupManager.Instance != null && chosenItem != null)
        {
            OptionPopupManager.Instance.ShowPopup(
                "Quieres comprar " + chosenItem.itemName + " por " + chosenItem.buyPrice + " Pesetas?",
                new Dictionary<string, System.Action> {
                    { "Si", () => {
                        int currentGold = StatManager.Instance.GetCurrentValue(ItemSO.StatType.gold);

                        if (currentGold >= chosenItem.buyPrice)
                        {
                            StatManager.Instance.ChangeStat(ItemSO.StatType.gold, -chosenItem.buyPrice);
                            InventoryManager.Instance.AddItem(chosenItem, 1);

                            if (spawnedModel != null)
                                Destroy(spawnedModel);

                            // After buying, pedestal stays empty until next visit
                            chosenItem = null;
                        }
                    }},
                    { "No", () => {}}
                }
            );
        }
    }
}
