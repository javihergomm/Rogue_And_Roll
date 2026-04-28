using UnityEngine;
using System;
using System.Collections.Generic;

/*
 * Manages entering and exiting the shop, enabling and disabling gameplay systems,
 * and resuming pending player movement after leaving the shop if needed.
 */
public class ShopExitManager : MonoBehaviour
{
    [Header("References (assign in Inspector)")]
    [SerializeField] private List<GameObject> shopPedestals = new();
    [SerializeField] private List<GameObject> decisionEmpties = new();
    [SerializeField] private GameObject ghostSpawnRoot;

    [Header("Ghosts (Shop Decoration)")]
    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private int ghostCount = 5;
    [SerializeField] private Transform ghostSpawnCenter;
    [SerializeField] private float ghostSpawnRadius = 3f;

    private List<GameObject> activeGhosts = new();

    [Header("Shop State")]
    [SerializeField] private bool inShop = false;
    private bool hasTriggeredFirstShopUnlock = false;

    [Header("Ouija Pointer")]
    [SerializeField] private GameObject tableroOuijaPuntero;

    [Header("Fixed Pointer Position")]
    [SerializeField] private Vector3 punteroFixedLocalPos;
    [SerializeField] private Vector3 punteroFixedLocalRot;

    [Header("Animator")]
    [SerializeField] private Animator animator;

    private Vector3 punteroInitialLocalPos;
    private Quaternion punteroInitialLocalRot;

    public event Action<bool> OnShopStateChanged;

    private void Start()
    {
        if (tableroOuijaPuntero != null)
        {
            punteroInitialLocalPos = tableroOuijaPuntero.transform.localPosition;
            punteroInitialLocalRot = tableroOuijaPuntero.transform.localRotation;
        }

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

    public void EnterShop()
    {
        if (inShop)
            return;

        inShop = true;

        if (animator != null)
            animator.SetTrigger("TiendaEntrar");

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

        Movement playerMovement = FindFirstObjectByType<Movement>(FindObjectsInactive.Include);
        if (playerMovement != null)
            playerMovement.enabled = false;

        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = false;

        if (tableroOuijaPuntero != null)
        {
            tableroOuijaPuntero.transform.SetLocalPositionAndRotation(
                punteroInitialLocalPos,
                punteroInitialLocalRot
            );
        }

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

    public void OnEnterStart()
    {
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

    public void OnEnterEnd()
    {
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(true);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(true);

        if (tableroOuijaPuntero != null)
        {
            tableroOuijaPuntero.SetActive(true);
            tableroOuijaPuntero.transform.localPosition = punteroFixedLocalPos;
            tableroOuijaPuntero.transform.localEulerAngles = punteroFixedLocalRot;
        }

        if (ghostSpawnRoot != null)
            ghostSpawnRoot.SetActive(true);

        SpawnGhosts();
        if (!hasTriggeredFirstShopUnlock && !Unlocks.IsUnlocked("item_dice_d4"))
        {
            Unlocks.Unlock("item_dice_d4");
            hasTriggeredFirstShopUnlock = true;
            Debug.Log("D4 desbloqueado al terminar la animación de entrada");
        }
    }

    public void OnExitStart()
    {
        ClearGhosts();
        if (ghostSpawnRoot != null)
            ghostSpawnRoot.SetActive(false);
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

        if (playerMovement != null)
        {
            if (playerMovement.pendingSteps > 0)
            {
                int steps = playerMovement.pendingSteps;
                playerMovement.pendingSteps = 0;

                // Reset movement state after exiting the shop
                playerMovement.ResetAfterShop();

                playerMovement.turnShouldEnd = true;
                playerMovement.StartMovingFixed(steps);
            }

            else
            {
                playerMovement.turnShouldEnd = true;
                TurnManager.Instance.ForcePlayerTurnEnd();
            }
        }
    }

    public void OnExitEnd()
    {
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(false);

        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(false);

        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(false);
    }

    private void SpawnGhosts()
    {
        ClearGhosts();

        // Select one ghost at random to be the special one
        int specialIndex = UnityEngine.Random.Range(0, ghostCount);

        for (int i = 0; i < ghostCount; i++)
        {
            GameObject g = Instantiate(ghostPrefab, ghostSpawnCenter.position, Quaternion.identity);

            if (g.TryGetComponent<GhostWander>(out var wander))
            {
                wander.center = ghostSpawnCenter;
                wander.maxDistance = ghostSpawnRadius;

                // Mark this ghost as the special one
                wander.isSpecial = (i == specialIndex);
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
