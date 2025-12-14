using UnityEngine;
using System.Collections.Generic;

/*
 * ShopPedestalRandomizer
 * ----------------------
 * Handles item display for a single shop pedestal.
 * Responsible for spawning a random item from the possible list,
 * showing it visually, and handling purchase confirmation via Ouija zones.
 */
public class ShopPedestalRandomizer : MonoBehaviour
{
    [Header("Possible items for this pedestal")]
    [SerializeField] private ItemSO[] possibleItems;

    [Header("Visuals")]
    [SerializeField] private Transform displayPoint;
    [SerializeField] private float displayYOffset = 0.1f;

    private ItemSO chosenItem;
    private GameObject spawnedModel;
    private bool hasGeneratedThisVisit = false;

    public static ShopPedestalRandomizer currentPedestal;
    public bool isAwaitingDecision = false;

    // Track items used this visit to avoid duplicates
    private static HashSet<ItemSO> usedItemsThisVisit = new HashSet<ItemSO>();

    private void Start()
    {
        RefreshItem();
        hasGeneratedThisVisit = true;
    }

    /*
     * Generate item if not already generated this visit.
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
     * Reset pedestal state for the next shop visit.
     */
    public void ResetForNextVisit()
    {
        hasGeneratedThisVisit = false;
        isAwaitingDecision = false;
        usedItemsThisVisit.Clear();
    }

    /*
     * Refresh the item displayed on this pedestal.
     * Picks a random item from the possible list, avoiding duplicates.
     */
    public void RefreshItem()
    {
        if (possibleItems == null || possibleItems.Length == 0)
        {
            Debug.LogWarning("No possible items assigned to ShopPedestalRandomizer!");
            return;
        }

        if (spawnedModel != null)
            Destroy(spawnedModel);

        List<ItemSO> availableItems = new List<ItemSO>();
        foreach (var item in possibleItems)
            if (!usedItemsThisVisit.Contains(item))
                availableItems.Add(item);

        if (availableItems.Count == 0)
        {
            Debug.LogWarning("No unique items left for pedestal!");
            chosenItem = null;
            return;
        }

        int index = Random.Range(0, availableItems.Count);
        chosenItem = availableItems[index];
        usedItemsThisVisit.Add(chosenItem);

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

            // Remove physics components from the spawned model
            foreach (var rb in spawnedModel.GetComponentsInChildren<Rigidbody>())
                Destroy(rb);
            foreach (var col in spawnedModel.GetComponentsInChildren<Collider>())
                Destroy(col);
        }
    }

    /*
     * Handles Ouija answer (Yes/No) for purchase confirmation.
     */
    public void HandleOuijaAnswer(OuijaAnswerZone.AnswerType answer)
    {
        Debug.Log("HandleOuijaAnswer called with: " + answer);
        if (!isAwaitingDecision) return;
        if (chosenItem == null) { isAwaitingDecision = false; return; }

        if (answer == OuijaAnswerZone.AnswerType.Yes)
        {
            int currentGold = StatManager.Instance.GetCurrentValue(ItemSO.StatType.gold);
            if (currentGold >= chosenItem.buyPrice)
            {
                StatManager.Instance.ChangeStat(ItemSO.StatType.gold, -chosenItem.buyPrice);
                InventoryManager.Instance.AddItem(chosenItem, 1);

                if (spawnedModel != null)
                    Destroy(spawnedModel);

                chosenItem = null;
                Debug.Log("Item purchased.");
            }
            else
            {
                Debug.Log("Not enough gold to buy " + chosenItem.itemName);
            }
        }
        else
        {
            Debug.Log("Purchase cancelled for " + chosenItem.itemName);
        }

        // Hide popup
        if (OptionPopupManager.Instance != null)
            OptionPopupManager.Instance.HidePopup();

        // Clear state
        isAwaitingDecision = false;
        if (currentPedestal == this)
            currentPedestal = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        currentPedestal = this;
        isAwaitingDecision = true;

        if (OptionPopupManager.Instance != null && chosenItem != null)
        {
            OptionPopupManager.Instance.ShowMessageOnly(
                "Do you want to buy " + chosenItem.itemName + " for " + chosenItem.buyPrice + " gold?\n" +
                "Move to YES or NO on the board."
            );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Only clear if no decision is pending
        if (!isAwaitingDecision && currentPedestal == this)
        {
            currentPedestal = null;
        }
    }
}
