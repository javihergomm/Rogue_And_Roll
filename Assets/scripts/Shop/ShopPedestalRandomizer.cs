using UnityEngine;
using System.Collections.Generic;

/*
 * ShopPedestalRandomizer
 * ----------------------
 * Handles item display for a single shop pedestal.
 * Ensures unique items across all pedestals per shop visit.
 * Only the first pedestal to initialize clears the used item list.
 */
public class ShopPedestalRandomizer : MonoBehaviour
{
    [Header("Possible items for this pedestal")]
    [SerializeField] private BaseItemSO[] possibleItems;

    [Header("Visuals")]
    [SerializeField] private Transform displayPoint;
    [SerializeField] private float displayYOffset = 0.1f;

    private BaseItemSO chosenItem;
    private GameObject spawnedModel;
    private bool hasGeneratedThisVisit = false;

    public static ShopPedestalRandomizer currentPedestal;
    public bool isAwaitingDecision = false;

    // Shared list of used items across all pedestals
    private static HashSet<BaseItemSO> usedItemsThisVisit = new HashSet<BaseItemSO>();

    private void Start()
    {
        // Only the first pedestal clears the used list
        if (usedItemsThisVisit.Count == 0)
            usedItemsThisVisit.Clear();

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
     * Does NOT clear the used item list (only the first pedestal does).
     */
    public void ResetForNextVisit()
    {
        hasGeneratedThisVisit = false;
        isAwaitingDecision = false;
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

        // Build list of items not used yet
        List<BaseItemSO> availableItems = new List<BaseItemSO>();
        foreach (var item in possibleItems)
        {
            if (!usedItemsThisVisit.Contains(item))
                availableItems.Add(item);
        }

        if (availableItems.Count == 0)
        {
            Debug.LogWarning("No unique items left for pedestal!");
            chosenItem = null;
            return;
        }

        // Pick a random unique item
        int index = Random.Range(0, availableItems.Count);
        chosenItem = availableItems[index];
        usedItemsThisVisit.Add(chosenItem);

        // Spawn the 3D model
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

            // Preserve original prefab scale
            spawnedModel.transform.localScale = chosenItem.prefab3D.transform.localScale;

            // Remove physics components
            foreach (var rb in spawnedModel.GetComponentsInChildren<Rigidbody>())
                Destroy(rb);
            foreach (var col in spawnedModel.GetComponentsInChildren<Collider>())
                Destroy(col);
        }
    }

    /*
     * Returns the chosen item for debugging or stress testing.
     */
    public BaseItemSO GetChosenItem()
    {
        return chosenItem;
    }

    /*
     * Handles Ouija answer (Yes/No) for purchase confirmation.
     */
    public void HandleOuijaAnswer(OuijaAnswerZone.AnswerType answer)
    {
        if (!isAwaitingDecision) return;
        if (chosenItem == null) { isAwaitingDecision = false; return; }

        if (answer == OuijaAnswerZone.AnswerType.Yes)
        {
            int currentGold = StatManager.Instance.GetCurrentValue(StatType.Gold);
            if (currentGold >= chosenItem.buyPrice)
            {
                StatManager.Instance.ChangeStat(StatType.Gold, -chosenItem.buyPrice);

                // Add item to inventory
                InventoryManager.Instance.AddItem(chosenItem, 1);

                if (spawnedModel != null)
                    Destroy(spawnedModel);

                chosenItem = null;
            }
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

        // Prevent opening a new popup if another transaction is in progress
        if (currentPedestal != null && currentPedestal.isAwaitingDecision)
            return;

        currentPedestal = this;
        isAwaitingDecision = true;

        if (OptionPopupManager.Instance != null && chosenItem != null)
        {
            OptionPopupManager.Instance.ShowMessageOnly(
                "Quieres comprar " + chosenItem.itemName +
                " por " + chosenItem.buyPrice + " Pesetas?\n" +
                "Muevete al SI o al NO en el tablero."
            );
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (!isAwaitingDecision && currentPedestal == this)
            currentPedestal = null;
    }
}
