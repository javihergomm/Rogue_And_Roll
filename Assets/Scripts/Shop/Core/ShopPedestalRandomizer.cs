using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 * Handles item selection, model spawning, and pedestal refresh logic.
 */
public class ShopPedestalRandomizer : MonoBehaviour
{
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
        EnsureSingleContainer();
        RefreshItem();
        hasGeneratedThisVisit = true;
    }


    private void EnsureSingleContainer()
    {
        List<Transform> containers = new();

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


    private void CreateContainer()
    {
        itemContainer = new GameObject("ItemContainer").transform;
        itemContainer.SetParent(transform);

        // OPTIMIZADO: una sola llamada
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
        EnsureSingleContainer();

        if (possibleItems == null || possibleItems.Length == 0)
            return;

        for (int i = itemContainer.childCount - 1; i >= 0; i--)
        {
            var child = itemContainer.GetChild(i);
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
        }

        spawnedModel = null;

        List<BaseItemSO> availableItems = new();

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
            EnsureSingleContainer();

        spawnedModel = Instantiate(chosenItem.Prefab3D, itemContainer);

        spawnedModel.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        spawnedModel.transform.localScale = Vector3.one;

        foreach (var rb in spawnedModel.GetComponentsInChildren<Rigidbody>())
            if (Application.isPlaying) Destroy(rb); else DestroyImmediate(rb);

        foreach (var col in spawnedModel.GetComponentsInChildren<Collider>())
            if (Application.isPlaying) Destroy(col); else DestroyImmediate(col);

        var center = spawnedModel.AddComponent<AutoCenterModel>();
        center.Normalize(chosenItem);

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
                StatManager.Instance.ChangeStat(StatType.Gold, -chosenItem.BuyPrice);
                InventoryManager.Instance.AddItem(chosenItem, 1);

                UsedItemsThisVisit.Add(chosenItem);

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
        EnsureSingleContainer();

        chosenItem = item;

        for (int i = itemContainer.childCount - 1; i >= 0; i--)
        {
            var child = itemContainer.GetChild(i);
            DestroyImmediate(child.gameObject);
        }

        SpawnModel();
        SceneView.RepaintAll();
    }

    public void EditorClearPreview()
    {
        EnsureSingleContainer();

        for (int i = itemContainer.childCount - 1; i >= 0; i--)
        {
            var child = itemContainer.GetChild(i);
            DestroyImmediate(child.gameObject);
        }

        SceneView.RepaintAll();
    }

    public void ForceRefreshForEditor()
    {
        EnsureSingleContainer();
        hasGeneratedThisVisit = false;
        RefreshItem();
        SceneView.RepaintAll();
    }
#endif
}
