using UnityEngine;
using System.Collections.Generic;

/*
 * ShopPedestalRandomizer
 * ----------------------
 * Selects a random item for the pedestal, prevents repeats,
 * scales and positions the 3D model, and handles purchase logic.
 */
public class ShopPedestalRandomizer : MonoBehaviour
{
    [Header("Possible items for this pedestal")]
    [SerializeField] private BaseItemSO[] possibleItems;

    [Header("Visual Settings")]
    [SerializeField] private float targetItemSize = 0.2f;
    [SerializeField] private float floatOffset = 0.05f;

    private BaseItemSO chosenItem;
    private GameObject spawnedModel;
    private Transform itemContainer;

    private bool hasGeneratedThisVisit = false;

    public static ShopPedestalRandomizer currentPedestal;
    public bool isAwaitingDecision = false;

    // Global memory for preventing repeats
    public static HashSet<BaseItemSO> UsedItemsThisVisit { get; private set; } = new HashSet<BaseItemSO>();
    public static HashSet<BaseItemSO> UsedItemsThisReroll { get; private set; } = new HashSet<BaseItemSO>();


    private void Start()
    {
        CreateContainer();
        RefreshItem();
        hasGeneratedThisVisit = true;
    }

    private void CreateContainer()
    {
        itemContainer = new GameObject("ItemContainer").transform;
        itemContainer.SetParent(transform);
        itemContainer.localPosition = Vector3.zero;
        itemContainer.localRotation = Quaternion.identity;
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
            if (!UsedItemsThisVisit.Contains(item) &&
                !UsedItemsThisReroll.Contains(item))
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

        UsedItemsThisReroll.Add(chosenItem);

        SpawnModel();
    }

    private bool IsFlatObject(Bounds b)
    {
        float min = Mathf.Min(b.size.x, b.size.y, b.size.z);
        float max = Mathf.Max(b.size.x, b.size.y, b.size.z);
        return min < max * 0.2f;
    }

    private Bounds GetLocalBounds(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0)
            return new Bounds(Vector3.zero, Vector3.one);

        Bounds combined = new Bounds(
            obj.transform.InverseTransformPoint(renderers[0].bounds.center),
            renderers[0].bounds.size
        );

        foreach (var r in renderers)
        {
            Vector3 localCenter = obj.transform.InverseTransformPoint(r.bounds.center);
            Bounds lb = new Bounds(localCenter, r.bounds.size);
            combined.Encapsulate(lb);
        }

        return combined;
    }

    private void SpawnModel()
    {
        if (chosenItem == null || chosenItem.Prefab3D == null)
            return;

        itemContainer.localPosition = Vector3.zero;

        spawnedModel = Instantiate(chosenItem.Prefab3D, itemContainer);
        spawnedModel.transform.localPosition = Vector3.zero;
        spawnedModel.transform.localRotation = Quaternion.identity;
        spawnedModel.transform.localScale = Vector3.one;

        foreach (var rb in spawnedModel.GetComponentsInChildren<Rigidbody>())
            Destroy(rb);
        foreach (var col in spawnedModel.GetComponentsInChildren<Collider>())
            Destroy(col);

        Bounds localBounds = GetLocalBounds(spawnedModel);

        bool isFlat = IsFlatObject(localBounds);

        // Local copies to avoid modifying serialized values
        float finalSize = targetItemSize;
        float finalOffset = floatOffset;

        if (isFlat)
        {
            spawnedModel.transform.localRotation = Quaternion.identity;

            if (localBounds.size.y < localBounds.size.z)
                spawnedModel.transform.localRotation = Quaternion.Euler(90, 0, 0);

            finalSize *= 1.4f;
            finalOffset *= 1.5f;

            localBounds = GetLocalBounds(spawnedModel);
        }

        float largest = Mathf.Max(localBounds.size.x, localBounds.size.y, localBounds.size.z);
        if (largest <= 0.0001f) largest = 1f;

        float scaleFactor = finalSize / largest;
        spawnedModel.transform.localScale = Vector3.one * scaleFactor;

        localBounds = GetLocalBounds(spawnedModel);

        float bottomY = localBounds.min.y;
        Vector3 lp = spawnedModel.transform.localPosition;
        lp.y -= bottomY;
        spawnedModel.transform.localPosition = lp;

        lp = spawnedModel.transform.localPosition;
        lp.y += finalOffset;
        spawnedModel.transform.localPosition = lp;

        MeshRenderer pedestalRenderer = GetComponentInChildren<MeshRenderer>();
        Vector3 topCenter = transform.position;

        if (pedestalRenderer != null)
        {
            Bounds pb = pedestalRenderer.bounds;
            topCenter = new Vector3(pb.center.x, pb.max.y, pb.center.z);
        }
        else
        {
            topCenter = transform.position + Vector3.up * 0.1f;
        }

        itemContainer.position = topCenter + Vector3.up * finalOffset;
    }

    public BaseItemSO GetChosenItem()
    {
        return chosenItem;
    }

    public void HandleOuijaAnswer(OuijaAnswerZone.AnswerType answer)
    {
        if (!isAwaitingDecision)
            return;

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

                UsedItemsThisVisit.Add(chosenItem);

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
        if (!other.CompareTag("Player"))
            return;

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
        if (!other.CompareTag("Player"))
            return;

        if (!isAwaitingDecision && currentPedestal == this)
            currentPedestal = null;
    }
}
