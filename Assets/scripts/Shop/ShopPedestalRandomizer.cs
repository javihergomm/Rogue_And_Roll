using UnityEngine;
using System.Collections.Generic;

/*
 * ShopPedestalRandomizer
 * ----------------------
 * Manages the item shown on a shop pedestal.
 * Selects an item from a list of possible items.
 * Avoids showing items already purchased during the current visit.
 * Avoids repeating items between pedestals during the same reroll.
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

    // Items purchased during the current shop visit
    private static HashSet<BaseItemSO> usedItemsThisVisit = new HashSet<BaseItemSO>();

    // Items selected during the current reroll
    private static HashSet<BaseItemSO> usedItemsThisReroll = new HashSet<BaseItemSO>();

    private void Start()
    {
        RefreshItem();
        hasGeneratedThisVisit = true;
    }

    /*
     * Clears the list of items used in the current reroll.
     * Called before generating new items after a reroll.
     */
    public static void PrepareForReroll()
    {
        usedItemsThisReroll.Clear();
    }

    /*
     * Generates an item if this pedestal has not generated one yet during this visit.
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
     * Resets pedestal state for the next shop visit.
     */
    public void ResetForNextVisit()
    {
        hasGeneratedThisVisit = false;
        isAwaitingDecision = false;
    }

    /*
     * Selects and displays an item.
     * Excludes items purchased during the visit.
     * Excludes items already used in the current reroll.
     */
    public void RefreshItem()
    {
        if (possibleItems == null || possibleItems.Length == 0)
            return;

        if (spawnedModel != null)
            Destroy(spawnedModel);

        List<BaseItemSO> availableItems = new List<BaseItemSO>();

        foreach (var item in possibleItems)
        {
            if (!usedItemsThisVisit.Contains(item) &&
                !usedItemsThisReroll.Contains(item))
            {
                availableItems.Add(item);
            }
        }

        if (availableItems.Count == 0)
        {
            chosenItem = null;
            return;
        }

        int index = Random.Range(0, availableItems.Count);
        chosenItem = availableItems[index];

        // Marks the item as used during this reroll
        usedItemsThisReroll.Add(chosenItem);

        // Spawns the item's 3D model
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
            spawnedModel.transform.localScale = chosenItem.prefab3D.transform.localScale;

            foreach (var rb in spawnedModel.GetComponentsInChildren<Rigidbody>())
                Destroy(rb);

            foreach (var col in spawnedModel.GetComponentsInChildren<Collider>())
                Destroy(col);
        }
    }

    /*
     * Returns the item currently assigned to this pedestal.
     */
    public BaseItemSO GetChosenItem()
    {
        return chosenItem;
    }

    /*
     * Handles the purchase confirmation result.
     * Deducts gold and adds the item to the inventory if confirmed.
     * Marks purchased items so they do not appear again during the visit.
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
                InventoryManager.Instance.AddItem(chosenItem, 1);

                usedItemsThisVisit.Add(chosenItem);

                if (spawnedModel != null)
                    Destroy(spawnedModel);

                chosenItem = null;
            }
        }

        if (OptionPopupManager.Instance != null)
            OptionPopupManager.Instance.HidePopup();

        isAwaitingDecision = false;
        if (currentPedestal == this)
            currentPedestal = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

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
