using UnityEngine;
using System;
using System.Collections.Generic;

/*
 * ShopExitManager
 * ----------------
 * This component controls the logic for entering and exiting the shop.
 * It handles:
 * - Enabling/disabling shop pedestals and decision objects
 * - Pausing enemies and hiding their visuals while inside the shop
 * - Disabling player movement and dice rolling
 * - Restoring everything when leaving the shop
 * - Triggering an animation ("Tienda") when entering or exiting
 */
public class ShopExitManager : MonoBehaviour
{
    [Header("References (assign in Inspector)")]
    [SerializeField] private List<GameObject> shopPedestals = new();
    [SerializeField] private List<GameObject> decisionEmpties = new();
    [SerializeField] private Transform boardTransform;

    [Header("Shop State")]
    [SerializeField] private bool inShop = false;

    [Header("Ouija Pointer")]
    [SerializeField] private GameObject tableroOuijaPuntero;

    [Header("Animator")]
    [SerializeField] private Animator animator; 

    private Vector3 punteroInitialLocalPos;
    private Quaternion punteroInitialLocalRot;

    public event Action<bool> OnShopStateChanged;

    private void Start()
    {
        // Store initial pointer transform so it can be restored later
        if (tableroOuijaPuntero != null)
        {
            punteroInitialLocalPos = tableroOuijaPuntero.transform.localPosition;
            punteroInitialLocalRot = tableroOuijaPuntero.transform.localRotation;
        }

        // If the game starts outside the shop, disable all shop-related objects
        if (!inShop)
        {
            foreach (var pedestal in shopPedestals)
                if (pedestal != null) pedestal.SetActive(false);

            foreach (var empty in decisionEmpties)
                if (empty != null) empty.SetActive(false);

            if (tableroOuijaPuntero != null)
                tableroOuijaPuntero.SetActive(false);
        }
    }

    // -------------------------------------------------------------------------
    // ENTER SHOP
    // -------------------------------------------------------------------------
    public void EnterShop()
    {
        if (inShop)
            return;

        inShop = true;

        // Trigger shop enter animation
        if (animator != null)
            animator.SetTrigger("TiendaEntrar");

        // Pause enemies and hide their visuals
        if (EnemyManager.Instance != null)
        {
            foreach (var enemy in EnemyManager.Instance.enemies)
            {
                if (enemy == null) continue;

                enemy.enabled = false;

                if (enemy.CupInstance != null)
                    enemy.CupInstance.SetActive(false);

                if (enemy.movement != null)
                    enemy.movement.gameObject.SetActive(false);
            }
        }

        OnShopStateChanged?.Invoke(true);

        // Disable player movement
        Movement playerMovement = FindFirstObjectByType<Movement>(FindObjectsInactive.Include);
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Disable dice rolling
        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = false;

        // Reset Ouija pointer and enable it
        if (tableroOuijaPuntero != null)
        {
            tableroOuijaPuntero.transform.SetLocalPositionAndRotation(
                punteroInitialLocalPos,
                punteroInitialLocalRot
            );
            tableroOuijaPuntero.SetActive(true);
        }

        // Enable shop pedestals and decision objects
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(true);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(true);

        // Prepare pedestals for new shop visit
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

        OnShopStateChanged?.Invoke(true);
    }

    // -------------------------------------------------------------------------
    // EXIT SHOP
    // -------------------------------------------------------------------------
    public void ConfirmExit()
    {
        if (!inShop)
            return;

        inShop = false;

        // Trigger shop exit animation
        if (animator != null)
            animator.SetTrigger("TiendaSalir");
            animator.SetTrigger("Permanecer");
        // Re-enable player movement
        Movement playerMovement = FindFirstObjectByType<Movement>(FindObjectsInactive.Include);
        if (playerMovement != null)
            playerMovement.enabled = true;

        // Re-enable dice rolling
        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = true;

        // Re-enable enemies and their visuals
        if (EnemyManager.Instance != null)
        {
            foreach (var enemy in EnemyManager.Instance.enemies)
            {
                if (enemy == null) continue;

                enemy.enabled = true;

                if (enemy.CupInstance != null)
                    enemy.CupInstance.SetActive(true);

                if (enemy.movement != null)
                    enemy.movement.gameObject.SetActive(true);
            }
        }

        // Hide Ouija pointer
        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(false);

        // Disable shop objects
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(false);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(false);

        // Reset rerolls
        if (StatManager.Instance != null)
        {
            int currentRerolls = StatManager.Instance.GetCurrentValue(StatType.ShopRerolls);
            if (currentRerolls > 0)
                StatManager.Instance.ChangeStat(StatType.ShopRerolls, -currentRerolls);
        }

        OnShopStateChanged?.Invoke(false);
    }

    // -------------------------------------------------------------------------
    // EXIT CONFIRMATION POPUP
    // -------------------------------------------------------------------------
    public void TriggerGoodbye()
    {
        if (!inShop)
            return;

        // Show popup with confirm/cancel callbacks
        PopupHelpers.ShowExitShopPopup(
            () => ConfirmExit(),
            () => CancelExit()
        );
    }

    // -------------------------------------------------------------------------
    // CANCEL EXIT (stay inside shop)
    // -------------------------------------------------------------------------
    public void CancelExit()
    {
        inShop = true;

        // Restore shop objects
        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(true);

        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(true);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(true);

        OnShopStateChanged?.Invoke(true);
    }

    // Returns whether the player is currently inside the shop
    public bool IsInShop()
    {
        return inShop;
    }
}
