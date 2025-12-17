using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

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
    [SerializeField] private bool inShop = true; // starts as true, editable in inspector

    private void Update()
    {
        // Only allow reroll if inside the shop
        if (!inShop) return;

        // Check if the configured key was pressed this frame
        if (Keyboard.current[rerollKey].wasPressedThisFrame)
        {
            TryRerollAllPedestals();
        }
    }

    /*
     * Attempts to reroll all pedestals in the shop.
     * Checks gold cost and available shop rerolls before executing.
     */
    private void TryRerollAllPedestals()
    {
        // Check if there are shop rerolls available
        int shopRerolls = StatManager.Instance.GetCurrentValue(StatType.ShopRerolls);
        if (shopRerolls <= 0)
        {
            Debug.Log("No shop rerolls remaining.");
            return;
        }

        // Check if the player has enough gold
        int currentGold = StatManager.Instance.GetCurrentValue(StatType.Gold);
        if (currentGold < globalRerollCost)
        {
            Debug.Log("Not enough gold to reroll the shop.");
            return;
        }

        // Deduct gold and consume one shop reroll
        StatManager.Instance.ChangeStat(StatType.Gold, -globalRerollCost);
        StatManager.Instance.UseShopReroll();

        // Find all shop pedestals and refresh them
        var pedestals = Object.FindObjectsByType<ShopPedestalRandomizer>(FindObjectsSortMode.None);
        foreach (var pedestal in pedestals)
        {
            pedestal.ResetForNextVisit();
            pedestal.GenerateIfNeeded();
        }

        Debug.Log("Shop rerolled. Shop rerolls remaining: " +
                  StatManager.Instance.GetCurrentValue(StatType.ShopRerolls));
    }
}
