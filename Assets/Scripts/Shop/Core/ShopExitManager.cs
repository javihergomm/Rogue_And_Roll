using UnityEngine;
using System;
using System.Collections.Generic;

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
    [SerializeField] private bool inShop = true;

    // SOLO NECESARIO MIENTRAS EL JUEGO EMPIEZA EN LA TIENDA
    // Cuando el juego empiece directamente en el tablero, puedes BORRAR esta variable.
    private bool firstTimeExit = true;

    [Header("Ouija Pointer")]
    [SerializeField] private GameObject tableroOuijaPuntero;

    public event Action<bool> OnShopStateChanged;

    // Enter shop mode: enable pedestals, restore rerolls, rotate board
    public void EnterShop()
    {
        if (inShop) return;
        inShop = true;

        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(true);

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

        OnShopStateChanged?.Invoke(true);
    }

    // Called when the player chooses to exit the shop
    public void TriggerGoodbye()
    {
        if (!inShop) return;

        PopupHelpers.ShowExitShopPopup(
            () => ConfirmExit(),
            () => CancelExit()
        );
    }

    // Confirm exit: disable pedestals, clear rerolls, rotate board back
    public void ConfirmExit()
    {
        if (!inShop) return;
        inShop = false;

        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(false);

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

        // SOLO NECESARIO MIENTRAS EL JUEGO EMPIEZA EN LA TIENDA
        if (firstTimeExit)
        {
            firstTimeExit = false;
            CharacterSelectManager.Instance.ShowSelector();
        }

        OnShopStateChanged?.Invoke(false);
    }

    // Cancel exit and stay inside the shop
    public void CancelExit()
    {
        inShop = true;

        if (tableroOuijaPuntero != null)
            tableroOuijaPuntero.SetActive(true);

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
