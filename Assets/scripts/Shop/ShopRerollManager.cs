using UnityEngine;
using UnityEngine.InputSystem; // Required for the new Input System

/*
 * ShopRerollManager
 * -----------------
 * Handles global reroll input for the shop.
 * Allows the player to reroll all pedestals at once using a customizable hotkey.
 * Default hotkey is 'R', but can be changed in the inspector.
 * 
 * Uses the new Input System (Keyboard.current).
 */
public class ShopRerollManager : MonoBehaviour
{
    [Header("Hotkey Settings")]
    [Tooltip("Key used to trigger a reroll of all shop pedestals.")]
    [SerializeField] private Key rerollKey = Key.R;

    [Header("Reroll Settings")]
    [Tooltip("Gold cost for rerolling all pedestals at once.")]
    [SerializeField] private int globalRerollCost = 20;

    [Tooltip("Maximum rerolls allowed per shop visit.")]
    [SerializeField] private int maxGlobalRerollsPerVisit = 2;

    private int globalRerollsUsedThisVisit = 0;

    private void Update()
    {
        // Check if the configured key was pressed this frame
        if (Keyboard.current[rerollKey].wasPressedThisFrame)
        {
            TryRerollAllPedestals();
        }
    }

    /*
     * Attempts to reroll all pedestals in the shop.
     * Checks gold cost and reroll limits before executing.
     */
    private void TryRerollAllPedestals()
    {
        if (globalRerollsUsedThisVisit >= maxGlobalRerollsPerVisit)
        {
            Debug.Log("Global reroll limit reached for this visit.");
            return;
        }

        int currentGold = StatManager.Instance.GetCurrentValue(StatType.Gold);
        if (currentGold < globalRerollCost)
        {
            Debug.Log("Not enough Pesetas to reroll the shop.");
            return;
        }

        // Deduct gold and increment reroll counter
        StatManager.Instance.ChangeStat(StatType.Gold, -globalRerollCost);
        globalRerollsUsedThisVisit++;

        // Find all pedestals and refresh them
        var pedestals = Object.FindObjectsByType<ShopPedestalRandomizer>(FindObjectsSortMode.None);
        foreach (var pedestal in pedestals)
        {
            pedestal.ResetForNextVisit();
            pedestal.GenerateIfNeeded();
        }

        Debug.Log("Shop rerolled. Global rerolls remaining: " +
                  (maxGlobalRerollsPerVisit - globalRerollsUsedThisVisit));
    }

    /*
     * Resets the reroll counter for the next shop visit.
     * Should be called when the player leaves or enters a new shop.
     */
    public void ResetForNextVisit()
    {
        globalRerollsUsedThisVisit = 0;
    }
}
