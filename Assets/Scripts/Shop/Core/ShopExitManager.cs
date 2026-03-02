using UnityEngine;
using System;
using System.Collections.Generic;

/*
 * ShopExitManager
 * ----------------
 * Handles entering and exiting the shop.
 * Enables and disables shop pedestals, decision zones, and UI elements.
 * Rotates the board depending on shop state.
 * Restores shop rerolls when entering the shop.
 * Resets pedestal state and global item memory so each shop visit generates new items.
 * Ensures the Ouija pointer always appears at its original position when entering the shop.
 */
public class ShopExitManager : MonoBehaviour
{
    [Header("References (assign in Inspector)")]
    [SerializeField] private List<GameObject> shopPedestals = new();
    [SerializeField] private List<GameObject> decisionEmpties = new();
    [SerializeField] private Transform boardTransform;

    [Header("Rotation Settings")]
    [SerializeField] private float exitRotationZ = 0f;
    [SerializeField] private float shopRotationZ = 180f;

    [Header("Shop State")]
    [SerializeField] private bool inShop = false;

    [Header("Ouija Pointer")]
    [SerializeField] private GameObject tableroOuijaPuntero;

    // Stores the initial local position and rotation of the Ouija pointer
    private Vector3 punteroInitialLocalPos;
    private Quaternion punteroInitialLocalRot;

    public event Action<bool> OnShopStateChanged;

    private void Start()
    {
        // Save the initial local position and rotation of the Ouija pointer
        if (tableroOuijaPuntero != null)
        {
            punteroInitialLocalPos = tableroOuijaPuntero.transform.localPosition;
            punteroInitialLocalRot = tableroOuijaPuntero.transform.localRotation;
        }

        // Initial shop state setup
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

        // Pause all enemies
        if (EnemyManager.Instance != null)
        {
            foreach (var enemy in EnemyManager.Instance.enemies)
            {
                if (enemy != null)
                    enemy.enabled = false;
            }
        }

        // Notify that the board should hide
        OnShopStateChanged?.Invoke(true);

        // Disable player interaction with the board
        Movement playerMovement = FindFirstObjectByType<Movement>(FindObjectsInactive.Include);
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = false;

        // Reset Ouija pointer to its original position and enable it
        if (tableroOuijaPuntero != null)
        {
            tableroOuijaPuntero.transform.localPosition = punteroInitialLocalPos;
            tableroOuijaPuntero.transform.localRotation = punteroInitialLocalRot;
            tableroOuijaPuntero.SetActive(true);
        }

        // Activate shop pedestals and decision zones
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(true);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(true);

        // Rotate board to shop orientation
        if (boardTransform != null)
        {
            Vector3 euler = boardTransform.eulerAngles;
            euler.z = shopRotationZ;
            boardTransform.eulerAngles = euler;
        }

        // Reset pedestal state and generate new items
        ShopPedestalRandomizer.PrepareForReroll();
        ShopPedestalRandomizer.ClearVisitMemory();

        foreach (var pedestalObj in shopPedestals)
        {
            if (pedestalObj == null) continue;

            if (pedestalObj.TryGetComponent<ShopPedestalRandomizer>(out var pedestal))
            {
                pedestal.ResetForNextVisit();
                pedestal.GenerateIfNeeded();
            }
        }

        // Notify listeners
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

        // Disable Ouija pointer
        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(false);

        // Disable pedestals and decision zones
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(false);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(false);

        // Remove remaining rerolls
        if (StatManager.Instance != null)
        {
            int currentRerolls = StatManager.Instance.GetCurrentValue(StatType.ShopRerolls);
            if (currentRerolls > 0)
                StatManager.Instance.ChangeStat(StatType.ShopRerolls, -currentRerolls);
        }

        // Rotate board back to normal orientation
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

        // Re-enable Ouija pointer
        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(true);

        // Re-enable pedestals and decision zones
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(true);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(true);

        // Rotate board to shop orientation
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
