using UnityEngine;
using UnityEngine.InputSystem;

/*
 * ShopRerollManager
 * -----------------
 * Handles global reroll input inside the shop.
 * Uses StatManager.ShopRerolls to track how many rerolls the player has.
 * Allows the player to reroll ALL pedestals at once using a hotkey.
 * Ensures pedestal state and global item memory are reset before generating new items.
 */
public class ShopRerollManager : MonoBehaviour
{
    [Header("Hotkey Settings")]
    [Tooltip("Key used to trigger a reroll of all shop pedestals.")]
    [SerializeField] private Key rerollKey = Key.R;

    [Header("Reroll Settings")]
    [Tooltip("Gold cost for rerolling all pedestals at once.")]
    [SerializeField] private int globalRerollCost = 2;

    [Header("Shop State")]
    [Tooltip("Indicates whether the player is currently inside the shop.")]
    [SerializeField] private bool inShop = true;

    private void Update()
    {
        // Only allow rerolling while inside the shop
        if (!inShop)
            return;

        // Check if the reroll hotkey was pressed this frame
        if (Keyboard.current[rerollKey].wasPressedThisFrame)
            TryRerollAllPedestals();
    }

    /*
     * TryRerollAllPedestals
     * ---------------------
     * Attempts to reroll all pedestals if:
     *  - The player has at least one shop reroll available
     *  - The player has enough gold to pay the reroll cost
     * 
     * If successful:
     *  - Gold is consumed
     *  - A shop reroll is consumed
     *  - Global item memory is cleared
     *  - All pedestals are regenerated
     */
    private void TryRerollAllPedestals()
    {
        // Check available rerolls
        int shopRerolls = StatManager.Instance.GetCurrentValue(StatType.ShopRerolls);
        if (shopRerolls <= 0)
            return;

        // Check gold
        int currentGold = StatManager.Instance.GetCurrentValue(StatType.Gold);
        if (currentGold < globalRerollCost)
            return;

        // Spend gold and consume a reroll
        StatManager.Instance.ChangeStat(StatType.Gold, -globalRerollCost);
        StatManager.Instance.UseShopReroll();

        // Reset global item memory so pedestals can generate new items
        ShopPedestalRandomizer.PrepareForReroll();
        ShopPedestalRandomizer.ClearVisitMemory();

        // Find all pedestals in the scene (active only)
        var pedestals = UnityEngine.Object.FindObjectsByType<ShopPedestalRandomizer>(FindObjectsInactive.Exclude);

        // Reset and regenerate each pedestal
        foreach (var pedestal in pedestals)
        {
            pedestal.ResetForNextVisit();
            pedestal.GenerateIfNeeded();
        }
    }

}
