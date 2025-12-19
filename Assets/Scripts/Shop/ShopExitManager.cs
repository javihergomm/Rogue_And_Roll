using UnityEngine;
using System.Collections.Generic;

public class ShopExitManager : MonoBehaviour
{
    [Header("References (assign in Inspector)")]
    [SerializeField] private List<GameObject> shopPedestals = new List<GameObject>();
    [SerializeField] private List<GameObject> decisionEmpties = new List<GameObject>();
    [SerializeField] private Transform boardTransform;

    [Tooltip("Reference to the cup (selector).")]
    [SerializeField] private GameObject cupObject;

    [Header("Selection Areas")]
    [Tooltip("List of all selection areas (Azul, Rojo, Amarillo, Verde).")]
    [SerializeField] private List<GameObject> selectionAreas = new List<GameObject>();

    [Header("Rotation Settings")]
    [SerializeField] private float exitRotationZ = 0f;
    [SerializeField] private float shopRotationZ = 180f;

    [Header("Shop State")]
    [SerializeField] private bool inShop = true;

    public void EnterShop()
    {
        if (inShop) return;
        inShop = true;

        Debug.Log("[ShopExitManager] Entering shop...");

        foreach (var pedestal in shopPedestals)
        {
            if (pedestal != null) pedestal.SetActive(true);
        }

        foreach (var empty in decisionEmpties)
        {
            if (empty != null) empty.SetActive(true);
        }

        if (cupObject != null) cupObject.SetActive(false); // hide cup in shop

        if (StatManager.Instance != null)
        {
            int maxRerolls = StatManager.Instance.GetMaxValue(StatType.ShopRerolls);
            StatManager.Instance.ChangeStat(StatType.ShopRerolls, maxRerolls);
        }

        if (boardTransform != null)
        {
            Vector3 euler = boardTransform.eulerAngles;
            euler.z = shopRotationZ;
            boardTransform.eulerAngles = euler;
        }
    }

    public void TriggerGoodbye()
    {
        if (!inShop) return;

        Debug.Log("[ShopExitManager] Goodbye triggered, showing confirmation popup...");

        OptionPopupManager.Instance.ShowExitShopPopup(
            () => ConfirmExit(),
            () => CancelExit()
        );
    }

    public void ConfirmExit()
    {
        if (!inShop)
        {
            Debug.Log("[ShopExitManager] ConfirmExit called but already outside shop.");
            return;
        }

        inShop = false;
        Debug.Log("[ShopExitManager] Exiting shop... inShop set to false");

        // Deactivate pedestals and shop empties
        foreach (var pedestal in shopPedestals)
        {
            if (pedestal != null)
            {
                pedestal.SetActive(false);
                Debug.Log("ConfirmExit: Deactivated pedestal " + pedestal.name);
            }
        }

        foreach (var empty in decisionEmpties)
        {
            if (empty != null)
            {
                empty.SetActive(false);
                Debug.Log("ConfirmExit: Deactivated empty " + empty.name);
            }
        }

        // Activate selection areas (Azul, Rojo, Amarillo, Verde)
        Debug.Log("ConfirmExit: selectionAreas count = " + selectionAreas.Count);
        foreach (var area in selectionAreas)
        {
            if (area != null)
            {
                area.SetActive(true);
                Debug.Log("ConfirmExit: Activated area " + area.name + " | Active? " + area.activeSelf);
            }
            else
            {
                Debug.LogWarning("ConfirmExit: Found null area in selectionAreas list");
            }
        }

        // Clear rerolls
        if (StatManager.Instance != null)
        {
            int currentRerolls = StatManager.Instance.GetCurrentValue(StatType.ShopRerolls);
            if (currentRerolls > 0)
            {
                StatManager.Instance.ChangeStat(StatType.ShopRerolls, -currentRerolls);
                Debug.Log("ConfirmExit: Cleared rerolls, removed " + currentRerolls);
            }
        }

        // Rotate board back
        if (boardTransform != null)
        {
            Vector3 euler = boardTransform.eulerAngles;
            euler.z = exitRotationZ;
            boardTransform.eulerAngles = euler;
            Debug.Log("ConfirmExit: Board rotated to " + exitRotationZ);
        }

        Debug.Log("[ShopExitManager] Shop closed, returning to game.");
    }


    public void CancelExit()
    {
        Debug.Log("[ShopExitManager] Exit cancelled, staying in shop.");
    }

    public bool IsInShop()
    {
        return inShop;
    }
}
