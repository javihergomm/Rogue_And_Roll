using UnityEngine;
using UnityEngine.InputSystem;

/*
 * ShopRerollManager
 * -----------------
 * Handles global reroll input for the shop.
 * Uses StatManager.ShopRerolls to track available rerolls.
 * Allows the player to reroll all pedestals at once using a customizable hotkey.
 */
public class ShopRerollManager : MonoBehaviour
{
    [Header("Hotkey Settings")]
    [Tooltip("Key used to trigger a reroll of all shop pedestals.")]
    [SerializeField] private Key rerollKey = Key.R;

    [Header("Reroll Settings")]
    [Tooltip("Gold cost for rerolling all pedestals at once.")]
    [SerializeField] private int globalRerollCost = 20;

    [Header("Shop State")]
    [Tooltip("Flag to indicate if the player is currently inside the shop.")]
    [SerializeField] private bool inShop = true;

    private void Update()
    {
        if (!inShop)
            return;

        if (Keyboard.current[rerollKey].wasPressedThisFrame)
            TryRerollAllPedestals();
    }

    /*
     * Attempts to reroll all pedestals in the shop.
     * Checks gold cost and available shop rerolls before executing.
     */
    private void TryRerollAllPedestals()
    {
        int shopRerolls = StatManager.Instance.GetCurrentValue(StatType.ShopRerolls);
        if (shopRerolls <= 0)
            return;

        int currentGold = StatManager.Instance.GetCurrentValue(StatType.Gold);
        if (currentGold < globalRerollCost)
            return;

        StatManager.Instance.ChangeStat(StatType.Gold, -globalRerollCost);
        StatManager.Instance.UseShopReroll();

        var pedestals = Object.FindObjectsByType<ShopPedestalRandomizer>(FindObjectsSortMode.None);
        foreach (var pedestal in pedestals)
        {
            pedestal.ResetForNextVisit();
            pedestal.GenerateIfNeeded();
        }
    }
}
