using UnityEngine;
using System;
using System.Collections.Generic;

/*
 * ShopExitManager
 * ----------------
 * Controls entering and exiting the shop.
 * Activates and deactivates shop pedestals, decision zones, and UI elements.
 * Adjusts the board rotation depending on shop state.
 * Restores available shop rerolls when entering the shop.
 * Resets pedestal state and global item memory so each shop visit generates new items.
 */
public class ShopExitManager : MonoBehaviour
{
    [Header("References (assign in Inspector)")]
    [SerializeField] private List<GameObject> shopPedestals = new List<GameObject>();
    [SerializeField] private List<GameObject> decisionEmpties = new List<GameObject>();
    [SerializeField] private Transform boardTransform;

    [Header("Rotation Settings")]
    [SerializeField] private float exitRotationZ = 0f;
    [SerializeField] private float shopRotationZ = 180f;

    [Header("Shop State")]
    [SerializeField] private bool inShop = false;

    [Header("Ouija Pointer")]
    [SerializeField] private GameObject tableroOuijaPuntero;

    public event Action<bool> OnShopStateChanged;

    private void Start()
    {
        if (!inShop)
        {
            foreach (var pedestal in shopPedestals)
                if (pedestal != null) pedestal.SetActive(false);

            foreach (var empty in decisionEmpties)
                if (empty != null) empty.SetActive(false);

            if (tableroOuijaPuntero != null)
                tableroOuijaPuntero.SetActive(false);

            if (boardTransform != null)
            {
                Vector3 euler = boardTransform.eulerAngles;
                euler.z = exitRotationZ;
                boardTransform.eulerAngles = euler;
            }
        }
    }

    public void EnterShop()
    {
        if (inShop)
            return;

        inShop = true;

        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(true);

        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(true);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(true);

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

        // Resets global item memory and pedestal state so each shop visit generates new items
        ShopPedestalRandomizer.PrepareForReroll();
        ShopPedestalRandomizer.ClearVisitMemory();

        foreach (var pedestalObj in shopPedestals)
        {
            if (pedestalObj == null) continue;

            var pedestal = pedestalObj.GetComponent<ShopPedestalRandomizer>();
            if (pedestal != null)
            {
                pedestal.ResetForNextVisit();
                pedestal.GenerateIfNeeded();
            }
        }

        OnShopStateChanged?.Invoke(true);
    }

    public void TriggerGoodbye()
    {
        if (!inShop)
            return;

        PopupHelpers.ShowExitShopPopup(
            () => ConfirmExit(),
            () => CancelExit()
        );
    }

    public void ConfirmExit()
    {
        if (!inShop)
            return;

        inShop = false;

        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(false);

        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(false);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(false);

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

        OnShopStateChanged?.Invoke(false);
    }

    public void CancelExit()
    {
        inShop = true;

        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(true);

        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(true);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(true);

        if (boardTransform != null)
        {
            Vector3 euler = boardTransform.eulerAngles;
            euler.z = shopRotationZ;
            boardTransform.eulerAngles = euler;
        }

        OnShopStateChanged?.Invoke(true);
    }

    public bool IsInShop()
    {
        return inShop;
    }
}
