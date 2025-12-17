using UnityEngine;
using System.Collections.Generic;

/*
 * ShopExitManager
 * ---------------
 * Handles entering and leaving the shop.
 * Responsibilities:
 * - Track shop state with the inShop flag
 * - Enable/disable multiple pedestals and empties (Yes/No/Goodbye)
 * - Reset or clear shop rerolls in StatManager
 * - Rotate the board to indicate shop/game state
 * - Show a confirmation popup when Goodbye is triggered (Spanish UI)
 */
public class ShopExitManager : MonoBehaviour
{
    [Header("References (assign in Inspector)")]
    [Tooltip("List of all pedestal GameObjects (buy/sell pedestals).")]
    [SerializeField] private List<GameObject> shopPedestals = new List<GameObject>();

    [Tooltip("List of empties for Yes/No/Goodbye zones.")]
    [SerializeField] private List<GameObject> decisionEmpties = new List<GameObject>();

    [Tooltip("Transform of the board that rotates when entering/leaving the shop.")]
    [SerializeField] private Transform boardTransform;

    [Header("Rotation Settings")]
    [Tooltip("Rotation angle (Z axis) when leaving the shop.")]
    [SerializeField] private float exitRotationZ = 0f;

    [Tooltip("Rotation angle (Z axis) when entering the shop.")]
    [SerializeField] private float shopRotationZ = 180f;

    [Header("Shop State")]
    [SerializeField] private bool inShop = true;

    // Called when entering the shop
    public void EnterShop()
    {
        if (inShop) return;
        inShop = true;

        Debug.Log("[ShopExitManager] Entering shop...");

        // Activate all pedestals
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(true);

        // Activate empties (Yes/No/Goodbye zones)
        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(true);

        // Reset rerolls
        if (StatManager.Instance != null)
        {
            int maxRerolls = StatManager.Instance.GetMaxValue(StatType.ShopRerolls);
            StatManager.Instance.ChangeStat(StatType.ShopRerolls, maxRerolls);
        }

        // Rotate board
        if (boardTransform != null)
        {
            Vector3 euler = boardTransform.eulerAngles;
            euler.z = shopRotationZ;
            boardTransform.eulerAngles = euler;
        }
    }

    // Called when Goodbye zone is triggered -> show confirmation popup
    public void TriggerGoodbye()
    {
        if (!inShop) return;

        Debug.Log("[ShopExitManager] Goodbye triggered, showing confirmation popup...");

        OptionPopupManager.Instance.ShowExitShopPopup(
            () => ConfirmExit(),
            () => CancelExit()
        );
    }

    // Called when player confirms exit
    public void ConfirmExit()
    {
        if (!inShop) return;
        inShop = false;

        Debug.Log("[ShopExitManager] Exiting shop...");

        // Deactivate all pedestals
        foreach (var pedestal in shopPedestals)
            if (pedestal != null) pedestal.SetActive(false);

        // Deactivate empties (Yes/No/Goodbye zones)
        foreach (var empty in decisionEmpties)
            if (empty != null) empty.SetActive(false);

        // Clear rerolls
        if (StatManager.Instance != null)
        {
            int currentRerolls = StatManager.Instance.GetCurrentValue(StatType.ShopRerolls);
            if (currentRerolls > 0)
                StatManager.Instance.ChangeStat(StatType.ShopRerolls, -currentRerolls);
        }

        // Rotate board back
        if (boardTransform != null)
        {
            Vector3 euler = boardTransform.eulerAngles;
            euler.z = exitRotationZ;
            boardTransform.eulerAngles = euler;
        }

        Debug.Log("[ShopExitManager] Shop closed, returning to game.");
    }

    // Called when player cancels exit
    public void CancelExit()
    {
        Debug.Log("[ShopExitManager] Exit cancelled, staying in shop.");
    }

    // Public getter for shop state
    public bool IsInShop() => inShop;
}
