using UnityEngine;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;


#if UNITY_EDITOR
using UnityEditor;
#endif

/*
 * ShopExitManager
 * ----------------
 * Handles entering and exiting the shop, enabling and disabling gameplay systems,
 * spawning ghosts, managing Ouija pointer behavior, and restoring pending movement
 * after leaving the shop.
 */
public class ShopExitManager : MonoBehaviour
{
    [Header("References (assign in Inspector)")]
    [SerializeField] private List<GameObject> shopPedestals = new();
    [SerializeField] private List<GameObject> decisionEmpties = new();
    [SerializeField] private GameObject ghostSpawnRoot;

    [Header("Ghosts")]
    [SerializeField] private GameObject normalGhostPrefab;
    [SerializeField] private GameObject specialGhostPrefab;

    [Header("Special Ghost Settings")]
    [SerializeField] private float specialGhostChance = 0.05f;

    [SerializeField] private int ghostCount = 5;
    [SerializeField] private Transform ghostSpawnCenter;
    [SerializeField] private float ghostSpawnRadius = 3f;

    private List<GameObject> activeGhosts = new();

    [Header("Shop State")]
    [SerializeField] private bool inShop = false;
    private bool hasTriggeredFirstShopUnlock = false;
    private bool exitAnimationFinished = false;

    [Header("Ouija Pointer")]
    [SerializeField] private GameObject tableroOuijaPuntero;

    [Header("Fixed Pointer Position")]
    [SerializeField] private Vector3 punteroFixedLocalPos;
    [SerializeField] private Vector3 punteroFixedLocalRot;
    private Vector3 punteroInitialLocalPos;
    private Quaternion punteroInitialLocalRot;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Lights")]
    [SerializeField] private GameObject normalLight;
    [SerializeField] private GameObject hellLight;

    public event Action<bool> OnShopStateChanged;

    private void Start()
    {
        // Cache initial pointer transform
        if (tableroOuijaPuntero != null)
        {
            punteroInitialLocalPos = tableroOuijaPuntero.transform.localPosition;
            punteroInitialLocalRot = tableroOuijaPuntero.transform.localRotation;
        }

        // Hide shop elements if starting outside the shop
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

    // ---------------------------------------------------------
    // ENTER SHOP
    // ---------------------------------------------------------
    public void EnterShop()
    {
        if (inShop)
            return;

        inShop = true;

        if (animator != null)
            animator.SetTrigger("TiendaEntrar");

        // Disable enemies but do NOT disable their GameObjects
        if (EnemyManager.Instance != null)
        {
            foreach (var enemy in EnemyManager.Instance.enemies)
            {
                if (enemy == null) continue;

                enemy.enabled = false;

                // Hide visuals only
                if (enemy.CupInstance != null)
                    enemy.CupInstance.SetActive(false);

                if (enemy.movement != null)
                    enemy.movement.enabled = false;
            }
        }

        // Disable player movement but do NOT disable the GameObject
        Movement playerMovement = Array.Find(
            Object.FindObjectsByType<Movement>(FindObjectsInactive.Include),
            m => m != null && m.isPlayer
            );

        if (playerMovement != null)
            playerMovement.enabled = false;

        // Disable dice system
        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = false;

        // Reset pointer
        if (tableroOuijaPuntero != null)
        {
            tableroOuijaPuntero.transform.SetLocalPositionAndRotation(
                punteroInitialLocalPos,
                punteroInitialLocalRot
            );
        }

        // ---------------------------------------------------------
        // NO REROLL on first visit
        // ---------------------------------------------------------
        if (hasTriggeredFirstShopUnlock)
        {
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
        }
        else
        {
            Debug.Log("[Shop] Primera visita: NO se hace reroll automático.");
        }


        OnShopStateChanged?.Invoke(true);
    }


    // ---------------------------------------------------------
    // EXIT CONFIRMATION
    // ---------------------------------------------------------
    public void ConfirmExit()
    {
        if (!inShop)
            return;

        ClearGhosts();

        if (ghostSpawnRoot != null)
            ghostSpawnRoot.SetActive(false);

        if (animator != null)
            animator.SetTrigger("TiendaSalir");
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

    public bool IsInShop()
    {
        return inShop;
    }

    // ---------------------------------------------------------
    // ENTER ANIMATION EVENTS
    // ---------------------------------------------------------
    public void OnEnterStart()
    {
        if (normalLight != null) normalLight.SetActive(false);
        if (hellLight != null) hellLight.SetActive(false);

        Movement playerMovement = Object.FindAnyObjectByType<Movement>();
        if (playerMovement != null)
            playerMovement.enabled = false;

        BridgeOfCatanEffect.HideVisual();

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

    public void OnEnterEnd()
    {
        if (hellLight != null) hellLight.SetActive(true);
        // Show pedestals and decision markers
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(true);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(true);

        // Activate pointer in fixed position
        if (tableroOuijaPuntero != null)
        {
            tableroOuijaPuntero.SetActive(true);
            tableroOuijaPuntero.transform.localPosition = punteroFixedLocalPos;
            tableroOuijaPuntero.transform.localEulerAngles = punteroFixedLocalRot;
        }

        // Spawn ghosts
        if (ghostSpawnRoot != null)
            ghostSpawnRoot.SetActive(true);

        SpawnGhosts();

        // First-time unlock
        if (!hasTriggeredFirstShopUnlock && !Unlocks.IsUnlocked("item_dice_d4"))
        {
            Unlocks.Unlock("item_dice_d4");
            hasTriggeredFirstShopUnlock = true;
        }
    }

    // ---------------------------------------------------------
    // EXIT ANIMATION EVENTS
    // ---------------------------------------------------------
    public void OnExitStart()
    {
        if (hellLight != null) hellLight.SetActive(false);
        ClearGhosts();

        if (ghostSpawnRoot != null)
            ghostSpawnRoot.SetActive(false);

        inShop = false;
        OnShopStateChanged?.Invoke(false);

        BridgeOfCatanEffect.ShowVisual();

        // Reactivate player movement component (but do NOT start movement yet)
        Movement playerMovement = Array.Find(
            Object.FindObjectsByType<Movement>(FindObjectsInactive.Include),
            m => m != null && m.isPlayer
        );

        if (playerMovement != null)
        {
            // Reactivate ONLY the component, never the GameObject
            playerMovement.enabled = true;
        }

        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = true;

        // Reactivate enemies (components only)
        if (EnemyManager.Instance != null)
        {
            foreach (var enemy in EnemyManager.Instance.enemies)
            {
                if (enemy == null) continue;

                enemy.enabled = true;

                if (enemy.CupInstance != null)
                    enemy.CupInstance.SetActive(true);

                if (enemy.movement != null)
                    enemy.movement.enabled = true;
            }
        }
    }

    public void OnExitEnd()
    {
        // Mark that the exit animation has fully finished
        exitAnimationFinished = true;

        // Restore normal lighting after leaving the shop
        if (normalLight != null) normalLight.SetActive(true);

        // Hide all shop pedestals
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(false);

        // Hide decision empties
        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(false);

        // Hide the Ouija pointer
        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(false);

        // --- Resume pending movement ONLY after the exit animation is fully done ---
        Movement playerMovement = Array.Find(
            Object.FindObjectsByType<Movement>(FindObjectsInactive.Include),
            m => m != null && m.isPlayer
        );

        if (playerMovement != null)
        {
            // Ensure the player becomes visible ONLY after the animation has ended
            if (exitAnimationFinished && playerMovement.gameObject != null)
                playerMovement.gameObject.SetActive(true);

            // If the player had pending movement saved before entering the shop,
            // resume it now that the exit animation is complete
            if (exitAnimationFinished && playerMovement.pendingSteps > 0)
            {
                int steps = playerMovement.pendingSteps;
                Debug.Log($"[ShopExit] pendingSteps READ on exit = {steps}");

                // Clear the pending steps so they are not reused
                playerMovement.pendingSteps = 0;
                Debug.Log("[ShopExit] pendingSteps RESET to 0");

                // Restore internal movement state and start moving again
                playerMovement.ResetAfterShop();
                playerMovement.StartMovingFixed(steps);
            }
        }
    }

    // ---------------------------------------------------------
    // GHOST SPAWNING
    // ---------------------------------------------------------
    private void SpawnGhosts()
    {
        ClearGhosts();

        bool spawnSpecial = UnityEngine.Random.value <= specialGhostChance;
        int specialIndex = spawnSpecial ? UnityEngine.Random.Range(0, ghostCount) : -1;

        for (int i = 0; i < ghostCount; i++)
        {
            bool isThisSpecial = (i == specialIndex);

            GameObject prefabToUse = isThisSpecial ? specialGhostPrefab : normalGhostPrefab;

            GameObject g = Instantiate(
                prefabToUse,
                ghostSpawnCenter.position,
                Quaternion.identity
            );

            if (g.TryGetComponent<GhostWander>(out var wander))
            {
                wander.center = ghostSpawnCenter;
                wander.maxDistance = ghostSpawnRadius;
                wander.isSpecial = isThisSpecial;
            }

            activeGhosts.Add(g);
        }
    }

    private void ClearGhosts()
    {
        foreach (var g in activeGhosts)
            if (g != null) Destroy(g);

        activeGhosts.Clear();
    }

#if UNITY_EDITOR
    /*
     * Editor-only preview helpers for testing shop layout and camera.
     */
    public void EditorPreviewShop()
    {
        Debug.Log("=== SHOP EDITOR PREVIEW ===");

        foreach (var pedestal in shopPedestals)
            if (pedestal != null)
                pedestal.SetActive(true);

        foreach (var pedestal in shopPedestals)
        {
            if (pedestal == null) continue;

            var comp = pedestal.GetComponent("ShopPedestal");
            if (comp != null)
            {
                var method = comp.GetType().GetMethod("EditorPreview");
                method?.Invoke(comp, null);
            }
        }

        var shopCameraPoint = GameObject.Find("ShopCameraPoint");
        if (Camera.main != null && shopCameraPoint != null)
        {
            Camera.main.transform.SetPositionAndRotation(
                shopCameraPoint.transform.position,
                shopCameraPoint.transform.rotation
            );
        }

        var player = GameObject.Find("Player");
        if (player != null)
            player.SetActive(false);

        var enemiesRoot = GameObject.Find("Enemies");
        if (enemiesRoot != null)
        {
            foreach (Transform child in enemiesRoot.transform)
                child.gameObject.SetActive(false);
        }

        Debug.Log("Shop preview activated in Scene View.");
    }

    public void EditorExitShop()
    {
        Debug.Log("=== SHOP EDITOR EXIT ===");

        foreach (var pedestal in shopPedestals)
            if (pedestal != null)
                pedestal.SetActive(false);

        foreach (var pedestal in shopPedestals)
        {
            if (pedestal == null) continue;

            var comp = pedestal.GetComponent("ShopPedestal");
            if (comp != null)
            {
                var method = comp.GetType().GetMethod("ClearPreview");
                method?.Invoke(comp, null);
            }
        }

        var player = GameObject.Find("Player");
        if (player != null)
            player.SetActive(true);

        var enemiesRoot = GameObject.Find("Enemies");
        if (enemiesRoot != null)
        {
            foreach (Transform child in enemiesRoot.transform)
                child.gameObject.SetActive(true);
        }

        var originalPoint = GameObject.Find("OriginalCameraPoint");
        if (Camera.main != null && originalPoint != null)
        {
            Camera.main.transform.SetPositionAndRotation(
                originalPoint.transform.position,
                originalPoint.transform.rotation
            );
        }

        Debug.Log("Shop editor preview exited. Scene restored.");
    }
#endif
}
