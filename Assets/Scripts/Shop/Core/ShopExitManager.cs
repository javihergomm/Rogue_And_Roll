using UnityEngine;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
 
    public static bool ShopIsInSellMode = false;
    public event Action<bool> OnShopStateChanged;

    private void Start()
    {
        // Stores the initial pointer transform so it can be restored later
        if (tableroOuijaPuntero != null)
        {
            punteroInitialLocalPos = tableroOuijaPuntero.transform.localPosition;
            punteroInitialLocalRot = tableroOuijaPuntero.transform.localRotation;
        }

        // If the player starts outside the shop, hide all shop elements
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

        // Plays the shop entrance animation
        if (animator != null)
            animator.SetTrigger("TiendaEntrar");

        // Disables all enemy logic while inside the shop
        if (EnemyManager.Instance != null)
        {
            foreach (var enemy in EnemyManager.Instance.enemies)
            {
                if (enemy == null) continue;

                enemy.enabled = false;

                if (enemy.CupInstance != null)
                    enemy.CupInstance.SetActive(false);

                if (enemy.movement != null)
                    enemy.movement.enabled = false;
            }
        }

        // Disables dice rolling while inside the shop
        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = false;

        // Resets the Ouija pointer to its initial position
        if (tableroOuijaPuntero != null)
        {
            tableroOuijaPuntero.transform.SetLocalPositionAndRotation(
                punteroInitialLocalPos,
                punteroInitialLocalRot
            );
        }

        // Prepares pedestal memory but DOES NOT generate items here.
        // Item generation happens only in OnEnterEnd().
        if (Unlocks.IsUnlocked("item_dice_d4"))
        {
            ShopPedestalRandomizer.PrepareForReroll();
            ShopPedestalRandomizer.ClearVisitMemory();

            foreach (var pedestalObj in shopPedestals)
            {
                if (pedestalObj == null) continue;

                if (pedestalObj.TryGetComponent<ShopPedestalRandomizer>(out var pedestal))
                {
                    pedestal.ResetForNextVisit();
                }
            }
        }
        else
        {
            Debug.Log("[Shop] First visit: NO automatic reroll.");
        }

        OnShopStateChanged?.Invoke(true);
    }

    // ---------------------------------------------------------
    // EXIT CONFIRMATION
    // ---------------------------------------------------------
    public void ConfirmExit()
    {
        // Ignore if the player is not currently inside the shop
        if (!inShop)
            return;

        // Remove all spawned ghosts
        ClearGhosts();

        // Hide the ghost spawn root
        if (ghostSpawnRoot != null)
            ghostSpawnRoot.SetActive(false);

        // Play the shop exit animation
        if (animator != null)
            animator.SetTrigger("TiendaSalir");
    }

    public void CancelExit()
    {
        // Cancels the exit and restores shop UI
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
        // Turn off both lights during the transition animation
        if (normalLight != null) normalLight.SetActive(false);
        if (hellLight != null) hellLight.SetActive(false);

        // Find the player's Movement component
        Movement playerMovement = Array.Find(
            Object.FindObjectsByType<Movement>(FindObjectsInactive.Include),
            m => m != null && m.isPlayer
        );

        // Pause player movement during the shop entrance animation
        if (playerMovement != null)
            playerMovement.pausedByShop = true;

        // Hide board visual effects
        BridgeOfCatanEffect.HideVisual();

        // Disable dice rolling while inside the shop
        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = false;

        // Disable all enemy logic and visuals
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
        // Turn on the hell light once the shop entrance animation finishes
        if (hellLight != null) hellLight.SetActive(true);

        // Show all pedestals
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(true);

        // Show all decision markers
        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(true);

        // Activate the Ouija pointer and place it in its fixed position
        if (tableroOuijaPuntero != null)
        {
            tableroOuijaPuntero.SetActive(true);
            tableroOuijaPuntero.transform.localPosition = punteroFixedLocalPos;
            tableroOuijaPuntero.transform.localEulerAngles = punteroFixedLocalRot;
        }

        // Activate the ghost root and spawn ghosts
        if (ghostSpawnRoot != null)
            ghostSpawnRoot.SetActive(true);

        SpawnGhosts();

        // Prepare pedestal memory and generate items (single reroll)
        ShopPedestalRandomizer.PrepareForReroll();
        ShopPedestalRandomizer.ClearVisitMemory();

        foreach (var pedestalObj in shopPedestals)
        {
            if (pedestalObj == null) continue;

            if (pedestalObj.TryGetComponent<ShopPedestalRandomizer>(out ShopPedestalRandomizer ped))
            {
                ped.ResetForNextVisit();
                ped.GenerateIfNeeded();
                ped.RefreshItem();
            }
        }

        // Unlock the reroll feature on the first visit
        if (!Unlocks.IsUnlocked("item_dice_d4"))
            Unlocks.Unlock("item_dice_d4");
    }

    // ---------------------------------------------------------
    // EXIT ANIMATION EVENTS
    // ---------------------------------------------------------
    public void OnExitStart()
    {
        // Turn off hell light and remove all ghosts
        if (hellLight != null) hellLight.SetActive(false);
        ClearGhosts();

        // Disable ghost spawn root
        if (ghostSpawnRoot != null)
            ghostSpawnRoot.SetActive(false);

        // Mark shop as exited and notify listeners
        inShop = false;
        OnShopStateChanged?.Invoke(false);

        // Show board visual effects again
        BridgeOfCatanEffect.ShowVisual();

        // Re-enable dice system
        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = true;

        // Disable all enemy logic until the exit animation fully ends
        if (EnemyManager.Instance != null)
        {
            foreach (var enemy in EnemyManager.Instance.enemies)
            {
                if (enemy == null) continue;

                enemy.enabled = false;

                if (enemy.CupInstance != null)
                    enemy.CupInstance.SetActive(false);

                if (enemy.movement != null)
                    enemy.movement.enabled = false;
            }
        }

        // Pause player movement during the exit animation
        Movement playerMovement = Array.Find(
            Object.FindObjectsByType<Movement>(FindObjectsInactive.Include),
            m => m != null && m.isPlayer
        );

        if (playerMovement != null)
            playerMovement.pausedByShop = true;
    }

    public void OnExitEnd()
    {
        // Restore normal lighting after leaving the shop
        if (normalLight != null) normalLight.SetActive(true);

        // Hide all shop pedestals
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(false);

        // Hide decision markers
        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(false);

        // Hide the Ouija pointer
        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(false);

        // Resume player movement after a short delay
        StartCoroutine(HandlePlayerExitAfterDelay());
    }

    private IEnumerator HandlePlayerExitAfterDelay()
    {
        // Wait a short moment to ensure the exit animation is visually finished
        yield return new WaitForSeconds(0.15f);

        // Find the player's Movement component
        Movement playerMovement = Array.Find(
            Object.FindObjectsByType<Movement>(FindObjectsInactive.Include),
            m => m != null && m.isPlayer
        );

        if (playerMovement != null)
        {
            // Ensure the player object is active again
            playerMovement.gameObject.SetActive(true);

            // Re-enable enemy logic now that the exit animation is done
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

            // Resume player movement after leaving the shop
            playerMovement.pausedByShop = false;

            // If the player had pending movement, resume it now
            if (playerMovement.pendingSteps > 0)
            {
                int steps = playerMovement.pendingSteps;
                playerMovement.pendingSteps = 0;

                playerMovement.ResetAfterShop();
                playerMovement.StartMovingFixed(steps);
                yield break;
            }

            // Otherwise, end the player's turn normally
            playerMovement.turnShouldEnd = true;
            TurnManager.Instance.ForcePlayerTurnEnd();
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

    // ---------------------------------------------------------
    // EDITOR ONLY
    // ---------------------------------------------------------
#if UNITY_EDITOR
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
