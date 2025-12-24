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

    [Header("Rotation Settings")]
    [SerializeField] private float exitRotationZ = 0f;
    [SerializeField] private float shopRotationZ = 180f;

    [Header("Shop State")]
    [SerializeField] private bool inShop = true;

    // Enter shop mode: enable pedestals, hide cup, restore rerolls, rotate board
    public void EnterShop()
    {
        if (inShop) return;
        inShop = true;

        foreach (var pedestal in shopPedestals)
        {
            if (pedestal != null)
                pedestal.SetActive(true);
        }

        foreach (var empty in decisionEmpties)
        {
            if (empty != null)
                empty.SetActive(true);
        }

        if (cupObject != null)
            cupObject.SetActive(false);

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

    // Called when the player chooses to exit the shop
    public void TriggerGoodbye()
    {
        if (!inShop) return;

        OptionPopupManager.Instance.ShowExitShopPopup(
            () => ConfirmExit(),
            () => CancelExit()
        );
    }

    // Confirm exit: disable pedestals, clear rerolls, rotate board back
    public void ConfirmExit()
    {
        if (!inShop) return;
        inShop = false;

        foreach (var pedestal in shopPedestals)
        {
            if (pedestal != null)
                pedestal.SetActive(false);
        }

        foreach (var empty in decisionEmpties)
        {
            if (empty != null)
                empty.SetActive(false);
        }

        if (StatManager.Instance != null)
        {
            int currentRerolls = StatManager.Instance.GetCurrentValue(StatType.ShopRerolls);
            if (currentRerolls > 0)
                StatManager.Instance.ChangeStat(StatType.ShopRerolls, -currentRerolls);
        }

        if (boardTransform != null)
        {
            Vector3 euler = boardTransform.eulerAngles;
            euler.z = exitRotationZ;
            boardTransform.eulerAngles = euler;
        }
    }

    // Cancel exit and stay inside the shop
    public void CancelExit()
    {
        // Ensure shop state remains active
        inShop = true;

        // Re-enable pedestals
        foreach (var pedestal in shopPedestals)
        {
            if (pedestal != null)
                pedestal.SetActive(true);
        }

        // Re-enable decision empties
        foreach (var empty in decisionEmpties)
        {
            if (empty != null)
                empty.SetActive(true);
        }

        // Cup stays hidden while inside the shop
        if (cupObject != null)
            cupObject.SetActive(false);

        // Keep board rotated to shop orientation
        if (boardTransform != null)
        {
            Vector3 euler = boardTransform.eulerAngles;
            euler.z = shopRotationZ;
            boardTransform.eulerAngles = euler;
        }
    }

    // Returns whether the player is currently inside the shop
    public bool IsInShop()
    {
        return inShop;
    }
}
