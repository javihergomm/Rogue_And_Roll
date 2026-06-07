using UnityEngine;
using System.Collections.Generic;
using static BaseItemSO;

/*
 * ShopPedestalRandomizer
 * ----------------------
 * Handles item selection, model spawning, and purchase confirmation.
 * Each pedestal displays one item and waits for the player to confirm
 * the purchase by moving to the YES or NO Ouija zones.
 */
public class ShopPedestalRandomizer : MonoBehaviour
{
    [SerializeField] private BaseItemSO[] possibleItems;

    private BaseItemSO chosenItem;
    private GameObject spawnedModel;
    private Transform itemContainer;

    private bool hasGeneratedThisVisit = false;

    // Reference to the pedestal currently waiting for a YES/NO decision
    public static ShopPedestalRandomizer currentPedestal;

    // True while waiting for the player to confirm the purchase
    public bool isAwaitingDecision = false;

    // Global flag to ensure only one pedestal can be in buying mode at a time
    public static bool buyingMode = false;

    // Memory of items used during this shop visit and reroll
    public static HashSet<BaseItemSO> UsedItemsThisVisit { get; private set; } = new HashSet<BaseItemSO>();
    public static HashSet<BaseItemSO> UsedItemsThisReroll { get; private set; } = new HashSet<BaseItemSO>();


    private void Start()
    {
        // Ensures the pedestal has exactly one container for the 3D model
        EnsureSingleContainer();

        // Generates the initial item for this pedestal
        RefreshItem();
        hasGeneratedThisVisit = true;
    }


    /*
     * Ensures that only one ItemContainer exists under this pedestal.
     * Removes duplicates if found.
     */
    private void EnsureSingleContainer()
    {
        List<Transform> containers = new List<Transform>();

        foreach (Transform t in GetComponentsInChildren<Transform>())
        {
            if (t.name == "ItemContainer")
                containers.Add(t);
        }

        if (containers.Count == 0)
        {
            CreateContainer();
            return;
        }

        itemContainer = containers[0];

        for (int i = 1; i < containers.Count; i++)
        {
            if (Application.isPlaying)
                Destroy(containers[i].gameObject);
            else
                DestroyImmediate(containers[i].gameObject);
        }
    }


    /*
     * Creates a new container for the 3D item model.
     */
    private void CreateContainer()
    {
        itemContainer = new GameObject("ItemContainer").transform;
        itemContainer.SetParent(transform);

        itemContainer.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        itemContainer.localScale = Vector3.one;
    }


    public static void PrepareForReroll()
    {
        UsedItemsThisReroll.Clear();
    }

    public static void ClearVisitMemory()
    {
        UsedItemsThisVisit.Clear();
    }


    /*
     * Generates the item only if it has not been generated yet during this visit.
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
     * Resets the pedestal state for the next shop visit.
     */
    public void ResetForNextVisit()
    {
        hasGeneratedThisVisit = false;
        isAwaitingDecision = false;
    }


    /*
     * Selects a valid item and spawns its 3D model on the pedestal.
     */
    public void RefreshItem()
    {
        EnsureSingleContainer();

        possibleItems ??= new BaseItemSO[0];

        // Removes any previous model
        for (int i = itemContainer.childCount - 1; i >= 0; i--)
        {
            var child = itemContainer.GetChild(i);
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
        }

        spawnedModel = null;

        List<BaseItemSO> availableItems = new List<BaseItemSO>();

        // Adds manually assigned items
        foreach (var item in possibleItems)
        {
            if (item == null) continue;
            if (item.Polarity == BaseItemSO.ItemPolarity.Especial) continue;
            if (!Unlocks.IsUnlocked(item.itemID)) continue;
            if (UsedItemsThisVisit.Contains(item)) continue;
            if (UsedItemsThisReroll.Contains(item)) continue;

            availableItems.Add(item);
        }

        // Loads items from Resources folders
        string[] folders = { "Dice", "Consumables", "Permanents", "LootBox" };

        foreach (var folder in folders)
        {
            var items = Resources.LoadAll<BaseItemSO>("Items/" + folder);

            foreach (var item in items)
            {
                if (item == null) continue;
                if (item.Polarity == BaseItemSO.ItemPolarity.Especial) continue;
                if (!Unlocks.IsUnlocked(item.itemID)) continue;
                if (UsedItemsThisVisit.Contains(item)) continue;
                if (UsedItemsThisReroll.Contains(item)) continue;
                if (availableItems.Contains(item)) continue;

                availableItems.Add(item);
            }
        }

        if (availableItems.Count == 0)
        {
            chosenItem = null;
            return;
        }

        // Selects a random item from the available list
        chosenItem = availableItems[Random.Range(0, availableItems.Count)];

        UsedItemsThisReroll.Add(chosenItem);

        SpawnModel();
    }


    /*
     * Instantiates the 3D model of the chosen item.
     */
    private void SpawnModel()
    {
        if (chosenItem == null || chosenItem.Prefab3D == null)
            return;

        if (itemContainer == null)
            EnsureSingleContainer();

        spawnedModel = Instantiate(chosenItem.Prefab3D, itemContainer);

        spawnedModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        spawnedModel.transform.localScale = Vector3.one;

        // Removes physics components from the model
        foreach (var rb in spawnedModel.GetComponentsInChildren<Rigidbody>())
            if (Application.isPlaying) Destroy(rb); else DestroyImmediate(rb);

        foreach (var col in spawnedModel.GetComponentsInChildren<Collider>())
            if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);

        // Centers the model visually
        var center = spawnedModel.AddComponent<AutoCenterModel>();
        center.Normalize(chosenItem);

        // Positions the model on top of the pedestal
        MeshRenderer pedestalRenderer = GetComponentInChildren<MeshRenderer>();
        Vector3 topCenter = transform.position;

        if (pedestalRenderer != null)
        {
            Bounds pb = pedestalRenderer.bounds;
            topCenter = new Vector3(pb.center.x, pb.max.y, pb.center.z);
        }

        itemContainer.position = topCenter;
    }


    public BaseItemSO GetChosenItem()
    {
        return chosenItem;
    }


    /*
     * Handles the YES/NO answer from the Ouija zones.
     * Applies the purchase if confirmed.
     */
    public void HandleOuijaAnswer(OuijaAnswerZone.AnswerType answer)
    {
        if (!isAwaitingDecision)
            return;

        if (chosenItem == null)
        {
            isAwaitingDecision = false;
            buyingMode = false;
            return;
        }

        // Applies the purchase if the player confirms
        if (answer == OuijaAnswerZone.AnswerType.Yes)
        {
            int currentGold = StatManager.Instance.GetCurrentValue(StatType.Gold);

            if (currentGold >= chosenItem.BuyPrice)
            {
                StatManager.Instance.ChangeStat(StatType.Gold, -chosenItem.BuyPrice);
                InventoryManager.Instance.AddItem(chosenItem, 1);

                UsedItemsThisVisit.Add(chosenItem);

                // Removes the model from the pedestal
                if (spawnedModel != null)
                {
                    if (Application.isPlaying)
                        Destroy(spawnedModel);
                    else
                        DestroyImmediate(spawnedModel);
                }

                chosenItem = null;
            }
        }

        // Hides the popup
        OptionPopupManager.Instance.HidePopup();

        // Clears state
        isAwaitingDecision = false;
        buyingMode = false;

        if (currentPedestal == this)
            currentPedestal = null;
    }


    /*
     * Triggered when the player enters the pedestal area.
     * Shows the purchase popup if allowed.
     */
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // Blocks purchase if selling mode is active
        if (SellPedestal.sellingMode)
            return;

        // Blocks purchase if another pedestal is already active
        if (buyingMode)
            return;

        // Activates buying mode
        buyingMode = true;

        // Blocks if another pedestal is awaiting a decision
        if (currentPedestal != null && currentPedestal.isAwaitingDecision)
            return;

        currentPedestal = this;
        isAwaitingDecision = true;

        // Shows the purchase popup
        if (OptionPopupManager.Instance != null && chosenItem != null)
        {
            OptionPopupManager.Instance.ShowMessage(
                "Quieres comprar " + chosenItem.ItemName +
                " por " + chosenItem.BuyPrice + " Pesetas?\n" +
                "Muevete al SI o al NO en el tablero."
            );
        }
    }


    /*
     * Triggered when the player leaves the pedestal area.
     * Clears the pedestal state if no decision is pending.
     */
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (!isAwaitingDecision && currentPedestal == this)
        {
            currentPedestal = null;
            buyingMode = false;
        }
    }

}
