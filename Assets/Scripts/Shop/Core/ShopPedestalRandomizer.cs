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

    private static HashSet<BaseItemSO> usedItemsThisVisit = new HashSet<BaseItemSO>();
    private static HashSet<BaseItemSO> usedItemsThisReroll = new HashSet<BaseItemSO>();

    private void Start()
    {
        RefreshItem();
        hasGeneratedThisVisit = true;
    }

    public static void PrepareForReroll()
    {
        usedItemsThisReroll.Clear();
    }

    public void GenerateIfNeeded()
    {
        if (!hasGeneratedThisVisit)
        {
            RefreshItem();
            hasGeneratedThisVisit = true;
        }
    }

    public void ResetForNextVisit()
    {
        hasGeneratedThisVisit = false;
        isAwaitingDecision = false;
    }

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

        usedItemsThisReroll.Add(chosenItem);

        SpawnModel();
    }

    private void SpawnModel()
    {
        if (chosenItem == null || chosenItem.Prefab3D == null || displayPoint == null)
            return;

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

        spawnedModel = Instantiate(chosenItem.Prefab3D, displayPoint);
        spawnedModel.transform.localPosition = localSpawnPos;
        spawnedModel.transform.localRotation = Quaternion.identity;
        spawnedModel.transform.localScale = chosenItem.Prefab3D.transform.localScale;

        foreach (var rb in spawnedModel.GetComponentsInChildren<Rigidbody>())
            Destroy(rb);

        foreach (var col in spawnedModel.GetComponentsInChildren<Collider>())
            Destroy(col);
    }

    public BaseItemSO GetChosenItem()
    {
        return chosenItem;
    }

    public void HandleOuijaAnswer(OuijaAnswerZone.AnswerType answer)
    {
        if (!isAwaitingDecision) return;
        if (chosenItem == null)
        {
            isAwaitingDecision = false;
            return;
        }

        if (answer == OuijaAnswerZone.AnswerType.Yes)
        {
            int currentGold = StatManager.Instance.GetCurrentValue(StatType.Gold);

            if (currentGold >= chosenItem.BuyPrice)
            {
                StatManager.Instance.ChangeStat(StatType.Gold, -chosenItem.BuyPrice);
                InventoryManager.Instance.AddItem(chosenItem, 1);

                usedItemsThisVisit.Add(chosenItem);

                if (spawnedModel != null)
                    Destroy(spawnedModel);

                chosenItem = null;
            }
        }

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
            OptionPopupManager.Instance.ShowMessage(
                "Quieres comprar " + chosenItem.ItemName +
                " por " + chosenItem.BuyPrice + " Pesetas?\n" +
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
