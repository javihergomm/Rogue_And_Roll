using UnityEngine;
using System;
using System.Collections.Generic;

public class ShopExitManager : MonoBehaviour
{
    [Header("References (assign in Inspector)")]
    [SerializeField] private List<GameObject> shopPedestals = new();
    [SerializeField] private List<GameObject> decisionEmpties = new();

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

        // If the game starts outside the shop, hide all shop-related objects
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

        // Trigger enter animation
        if (animator != null)
            animator.SetTrigger("TiendaEntrar");

        // Disable enemies and hide their visuals
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

        // Disable player movement
        Movement playerMovement = FindFirstObjectByType<Movement>(FindObjectsInactive.Include);
        if (playerMovement != null)
            playerMovement.enabled = false;

        // Disable dice rolling
        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = false;

        // Reset Ouija pointer transform
        if (tableroOuijaPuntero != null)
        {
            tableroOuijaPuntero.transform.SetLocalPositionAndRotation(
                punteroInitialLocalPos,
                punteroInitialLocalRot
            );
        }

        // Prepare pedestals for a new shop visit
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

        // Trigger exit animation (speed = -1)
        if (animator != null)
            animator.SetTrigger("TiendaSalir");

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

    // -------------------------------------------------------------------------
    // TriggerGoodbye: shows a confirmation popup before exiting the shop
    // -------------------------------------------------------------------------
    public void TriggerGoodbye()
    {
        if (!inShop)
            return;

        PopupHelpers.ShowExitShopPopup(
            () => ConfirmExit(),
            () => CancelExit()
        );
    }

    public bool IsInShop()
    {
        return inShop;
    }

    // -------------------------------------------------------------------------
    // ANIMATION EVENTS
    // -------------------------------------------------------------------------

    // Called at frame 0 of the "TiendaEntrar" animation
    public void OnEnterStart()
    {
        // Hide player movement, dice, and enemies
        Movement playerMovement = FindFirstObjectByType<Movement>(FindObjectsInactive.Include);
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = false;

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
    }

    // Called at the last frame of the "TiendaEntrar" animation
    public void OnEnterEnd()
    {
        // Show pedestals, decision empties, and pointer
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(true);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(true);

        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(true);
    }

    // Called at frame 0 of the "TiendaSalir" animation (played backwards)
    public void OnExitStart()
    {
        // This runs at the END of the exit animation because speed = -1

        inShop = false;
        OnShopStateChanged?.Invoke(false);

        Movement playerMovement = FindFirstObjectByType<Movement>(FindObjectsInactive.Include);
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = true;

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
    }

    // Called at the last frame of the "TiendaSalir" animation (played backwards)
    public void OnExitEnd()
    {
        // This runs at the BEGINNING of the exit animation because speed = -1

        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(false);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(false);

        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(false);
    }
}
