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
    [SerializeField] private Key rerollKey = Key.R;

    [Header("Reroll Settings")]
    [SerializeField] private int globalRerollCost = 2;

    [Header("Shop State")]
    [SerializeField] private bool inShop = true;

    private void Update()
    {
        if (!inShop)
            return;

        if (Keyboard.current[rerollKey].wasPressedThisFrame)
            TryRerollAllPedestals();
    }

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

        // Clear reroll memory
        ShopPedestalRandomizer.PrepareForReroll();

        var pedestals = Object.FindObjectsByType<ShopPedestalRandomizer>(FindObjectsSortMode.None);
        foreach (var pedestal in pedestals)
        {
            pedestal.ResetForNextVisit();
            pedestal.GenerateIfNeeded();
        }
    }
}
