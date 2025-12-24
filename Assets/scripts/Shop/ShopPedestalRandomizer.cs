using UnityEngine;
using System.Collections.Generic;

/*
 * ShopPedestalRandomizer
 * ----------------------
 * Handles item display for a single shop pedestal.
 * Responsibilities:
 * - Spawns a random item from the possible list.
 * - Ensures no duplicate items across pedestals in the same visit.
 * - Displays the item visually without changing its scale.
 * - Handles purchase confirmation via Ouija zones.
 * - Prevents opening/closing purchase popups if another transaction is already in progress.
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

    // Track items used this visit to avoid duplicates across pedestals
    private static HashSet<BaseItemSO> usedItemsThisVisit = new HashSet<BaseItemSO>();

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
     * Picks a random item from the possible list, avoiding duplicates across pedestals.
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

            // Preserve original prefab scale (do not resize)
            spawnedModel.transform.localScale = chosenItem.prefab3D.transform.localScale;

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
            int currentGold = StatManager.Instance.GetCurrentValue(StatType.Gold);
            if (currentGold >= chosenItem.buyPrice)
            {
                StatManager.Instance.ChangeStat(StatType.Gold, -chosenItem.buyPrice);

                // Aquí añadimos el objeto al inventario
                InventoryManager.Instance.AddItem(chosenItem, 1);

                if (spawnedModel != null)
                    Destroy(spawnedModel);

                chosenItem = null;
                Debug.Log("Item purchased.");
            }
            else
            {
                Debug.Log("Not enough Pesetas to buy " + chosenItem.itemName);
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

        // Prevent opening a new popup if another transaction is already in progress
        if (currentPedestal != null && currentPedestal.isAwaitingDecision)
        {
            Debug.Log("[ShopPedestal] Transaction already in progress, skipping popup.");
            return;
        }

        currentPedestal = this;
        isAwaitingDecision = true;

        if (OptionPopupManager.Instance != null && chosenItem != null)
        {
            // Player-facing text remains in Spanish
            OptionPopupManager.Instance.ShowMessageOnly(
                $"¿Quieres comprar {chosenItem.itemName} por {chosenItem.buyPrice} Pesetas?\n" +
                "Muévete al SÍ o al NO en el tablero."
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
