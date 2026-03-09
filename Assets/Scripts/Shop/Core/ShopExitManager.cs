using UnityEngine;
using System;
using System.Collections.Generic;

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

            if (boardTransform != null)
            {
                Vector3 euler = boardTransform.eulerAngles;
                euler.z = exitRotationZ;
                boardTransform.eulerAngles = euler;
            }
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

        // Pause all enemies + hide their objects
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

        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = false;

        // Reset Ouija pointer
        if (tableroOuijaPuntero != null)
        {
            tableroOuijaPuntero.transform.localPosition = punteroInitialLocalPos;
            tableroOuijaPuntero.transform.localRotation = punteroInitialLocalRot;
            tableroOuijaPuntero.SetActive(true);
        }

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

        // Re-enable player movement
        Movement playerMovement = FindFirstObjectByType<Movement>(FindObjectsInactive.Include);
        if (playerMovement != null)
            playerMovement.enabled = true;

        if (DiceRollManager.Instance != null)
            DiceRollManager.Instance.enabled = true;

        // Re-enable enemy objects + scripts
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

    public void TriggerGoodbye()
    {
        if (!inShop)
            return;

        PopupHelpers.ShowExitShopPopup(
            () => ConfirmExit(),
            () => CancelExit()
        );
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
