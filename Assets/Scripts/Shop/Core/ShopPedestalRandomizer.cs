using UnityEngine;
using System.Collections.Generic;

/*
 * ShopPedestalRandomizer
 * ----------------------
 * Selects a random item for the pedestal, prevents repeats,
 * spawns the 3D model, normalizes it using AutoCenterModel,
 * and positions the container on top of the pedestal.
 */
public class ShopPedestalRandomizer : MonoBehaviour
{
    [Header("Possible items for this pedestal")]
    [SerializeField] private BaseItemSO[] possibleItems;

    private BaseItemSO chosenItem;
    private GameObject spawnedModel;
    private Transform itemContainer;

    private bool hasGeneratedThisVisit = false;

    public static ShopPedestalRandomizer currentPedestal;
    public bool isAwaitingDecision = false;

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
        if (itemContainer != null)
            return;

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
        {
            if (Application.isPlaying)
                Destroy(spawnedModel);
            else
                DestroyImmediate(spawnedModel);
        }

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

    private void SpawnModel()
    {
        if (chosenItem == null || chosenItem.Prefab3D == null)
            return;

        if (itemContainer == null)
            CreateContainer();

        // Reset container
        itemContainer.localPosition = Vector3.zero;
        itemContainer.localRotation = Quaternion.identity;

        // Instantiate model
        spawnedModel = Instantiate(chosenItem.Prefab3D, itemContainer);
        spawnedModel.transform.localPosition = Vector3.zero;
        spawnedModel.transform.localRotation = Quaternion.identity;
        spawnedModel.transform.localScale = Vector3.one;

        // Remove physics
        foreach (var rb in spawnedModel.GetComponentsInChildren<Rigidbody>())
            if (Application.isPlaying) Destroy(rb); else DestroyImmediate(rb);

        foreach (var col in spawnedModel.GetComponentsInChildren<Collider>())
            if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);

        // Normalize model inside container
        var center = spawnedModel.AddComponent<AutoCenterModel>();
        center.Normalize(chosenItem);

        // Place container on top of pedestal
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
                // Pay and give item
                StatManager.Instance.ChangeStat(StatType.Gold, -chosenItem.BuyPrice);
                InventoryManager.Instance.AddItem(chosenItem, 1);

                // Mark item as used this visit
                UsedItemsThisVisit.Add(chosenItem);

                // Remove model
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

        // Hide popup
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

#if UNITY_EDITOR
    public void EditorPreview(BaseItemSO item)
    {
        if (itemContainer == null)
            CreateContainer();

        chosenItem = item;

        if (spawnedModel != null)
            DestroyImmediate(spawnedModel);

        SpawnModel();
        UnityEditor.SceneView.RepaintAll();
    }

    public void EditorClearPreview()
    {
        if (spawnedModel != null)
            DestroyImmediate(spawnedModel);

        UnityEditor.SceneView.RepaintAll();
    }
#endif
}
