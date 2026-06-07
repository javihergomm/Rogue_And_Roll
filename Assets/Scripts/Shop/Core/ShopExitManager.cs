using UnityEngine;
using System;
using System.Collections.Generic;
using Object = UnityEngine.Object;
using System.Collections;

/*
 * ShopExitManager
 * ---------------
 * Handles all logic related to entering and exiting the shop.
 */
public class ShopExitManager : MonoBehaviour
{
    [Header("References")]
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
    [SerializeField] private GameObject ouijaPointer;

    [Header("Fixed Pointer Position")]
    [SerializeField] private Vector3 pointerFixedLocalPos;
    [SerializeField] private Vector3 pointerFixedLocalRot;
    private Vector3 pointerInitialLocalPos;
    private Quaternion pointerInitialLocalRot;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    [Header("Shop Sounds")]
    [SerializeField] private SoundGenerator thunderEmitter;
    [SerializeField] private AudioClip thunderClip;

    [Header("Lights")]
    [SerializeField] private GameObject normalLight;
    [SerializeField] private GameObject hellLight;

    public static bool ShopIsInSellMode = false;
    public event Action<bool> OnShopStateChanged;

    private void Start()
    {
        // Store initial pointer transform
        if (ouijaPointer != null)
        {
            pointerInitialLocalPos = ouijaPointer.transform.localPosition;
            pointerInitialLocalRot = ouijaPointer.transform.localRotation;
        }

        // Hide shop elements if starting outside the shop
        if (!inShop)
        {
            foreach (var pedestal in shopPedestals)
                if (pedestal != null) pedestal.SetActive(false);

            foreach (var empty in decisionEmpties)
                if (empty != null) empty.SetActive(false);

            if (ouijaPointer != null)
                ouijaPointer.SetActive(false);
        }
    }

    // ---------------------------------------------------------
    // ENTER SHOP (manual, via UI button)
    // ---------------------------------------------------------
    public void EnterShop()
    {
        if (inShop)
            return;

        inShop = true;

        // Play entrance animation
        if (animator != null)
            animator.SetTrigger("TiendaEntrar");

        // Disable enemy logic
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

        // Disable dice system while inside the shop
        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = false;

        // Reset pointer to initial position
        if (ouijaPointer != null)
        {
            ouijaPointer.transform.SetLocalPositionAndRotation(
                pointerInitialLocalPos,
                pointerInitialLocalRot
            );
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

        if (ouijaPointer != null)
            ouijaPointer.SetActive(true);

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

        if (thunderEmitter != null && thunderClip != null)
            thunderEmitter.PlayExternalOneShot(thunderClip);

        Movement playerMovement = FindAnyObjectByType<Movement>(FindObjectsInactive.Include);

        if (playerMovement != null)
            playerMovement.pausedByShop = true;

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

        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(true);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(true);

        if (ouijaPointer != null)
        {
            ouijaPointer.SetActive(true);
            ouijaPointer.transform.localPosition = pointerFixedLocalPos;
            ouijaPointer.transform.localEulerAngles = pointerFixedLocalRot;
        }

        if (ghostSpawnRoot != null)
            ghostSpawnRoot.SetActive(true);

        SpawnGhosts();
    }

    // ---------------------------------------------------------
    // EXIT ANIMATION EVENTS
    // ---------------------------------------------------------
    public void OnExitStart()
    {
        ClearGhosts();

        if (normalLight != null) normalLight.SetActive(true);

        if (ghostSpawnRoot != null)
            ghostSpawnRoot.SetActive(false);

        inShop = false;
        OnShopStateChanged?.Invoke(false);

        BridgeOfCatanEffect.ShowVisual();

        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = true;

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

        Movement playerMovement = FindAnyObjectByType<Movement>(FindObjectsInactive.Include);

        if (playerMovement != null)
        {
            playerMovement.pausedByShop = false;
            playerMovement.pendingSteps = 0;
            playerMovement.turnShouldEnd = false;
        }
    }


    public void OnExitEnd()
    {
        if (hellLight != null) hellLight.SetActive(false);

        if (thunderEmitter != null && thunderClip != null)
            thunderEmitter.PlayExternalOneShot(thunderClip);

        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(false);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(false);

        if (ouijaPointer != null)
            ouijaPointer.SetActive(false);

        
        Movement playerMovement = FindAnyObjectByType<Movement>(FindObjectsInactive.Include);
        if (playerMovement != null)
        {
            playerMovement.pendingSteps = 0;
            playerMovement.turnShouldEnd = false;
            playerMovement.turnShouldEnd = false;
        }

        StartCoroutine(HandlePlayerExitAfterDelay());
    }

    private IEnumerator HandlePlayerExitAfterDelay()
    {
        yield return new WaitForSeconds(0.15f);

        Movement playerMovement = FindAnyObjectByType<Movement>(FindObjectsInactive.Include);

        if (playerMovement != null)
        {
            playerMovement.gameObject.SetActive(true);

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

            playerMovement.pausedByShop = false;

            // Resume pending movement ONLY if the player entered via checkpoint
            if (playerMovement.pendingSteps > 0)
            {
                int steps = playerMovement.pendingSteps;
                playerMovement.pendingSteps = 0;

                playerMovement.ResetAfterShop();
                playerMovement.StartMovingFixed(steps);
                yield break;
            }

            // No pending movement: do NOT end the turn.
            // The player will manually roll the dice.
            playerMovement.turnShouldEnd = false;
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
            bool isSpecial = (i == specialIndex);

            GameObject prefab = isSpecial ? specialGhostPrefab : normalGhostPrefab;

            GameObject g = Instantiate(
                prefab,
                ghostSpawnCenter.position,
                Quaternion.identity
            );

            if (g.TryGetComponent<GhostWander>(out var wander))
            {
                wander.center = ghostSpawnCenter;
                wander.maxDistance = ghostSpawnRadius;
                wander.isSpecial = isSpecial;
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
}
