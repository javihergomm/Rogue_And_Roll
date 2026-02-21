using UnityEngine;
using UnityEngine.InputSystem;

/*
 * ShopRerollManager
 * -----------------
 * Handles global reroll input for the shop.
 * Uses StatManager.ShopRerolls to track available rerolls.
 * Allows the player to reroll all pedestals at once using a customizable hotkey.
 * Ensures pedestal state and global item memory are fully reset before rerolling.
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

    private void TryRerollAllPedestals()
    {
        int shopRerolls = StatManager.Instance.GetCurrentValue(StatType.ShopRerolls);
        if (shopRerolls <= 0)
            return;

        int currentGold = StatManager.Instance.GetCurrentValue(StatType.Gold);
        if (currentGold < globalRerollCost)
            return;

        // Spend gold and consume a reroll
        StatManager.Instance.ChangeStat(StatType.Gold, -globalRerollCost);
        StatManager.Instance.UseShopReroll();

        // Reset global item memory
        ShopPedestalRandomizer.PrepareForReroll();
        ShopPedestalRandomizer.ClearVisitMemory();

        // Reset and regenerate each pedestal
        var pedestals = Object.FindObjectsByType<ShopPedestalRandomizer>(FindObjectsSortMode.None);
        foreach (var pedestal in pedestals)
        {
            pedestal.ResetForNextVisit();
            pedestal.GenerateIfNeeded();
        }
    }

#if UNITY_EDITOR
    public void EditorForceReroll()
    {
        ShopPedestalRandomizer.PrepareForReroll();
        ShopPedestalRandomizer.ClearVisitMemory();

        var pedestals = Object.FindObjectsByType<ShopPedestalRandomizer>(FindObjectsSortMode.None);

        foreach (var pedestal in pedestals)
        {
            pedestal.ResetForNextVisit();
            pedestal.GenerateIfNeeded();
        }

        UnityEditor.SceneView.RepaintAll();
    }

    public void EditorClearAll()
    {
        var pedestals = Object.FindObjectsByType<ShopPedestalRandomizer>(FindObjectsSortMode.None);

        foreach (var pedestal in pedestals)
            pedestal.EditorClearPreview();

        UnityEditor.SceneView.RepaintAll();
    }
#endif
}
