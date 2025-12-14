using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [SerializeField] private ShopPedestalRandomizer[] pedestals;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Called when the player reaches the shop tile on the board
    public void EnterShop()
    {
        // Generate items only once per visit
        foreach (var pedestal in pedestals)
            pedestal.GenerateIfNeeded();

        // Here you trigger your board rotation animation
        // Example:
        // BoardRotator.Instance.RotateToShop();
    }

    // Called when the player leaves the shop
    public void ExitShop()
    {
        // Reset pedestals for next visit
        foreach (var pedestal in pedestals)
            pedestal.ResetForNextVisit();

        // Rotate board back to game mode
        // BoardRotator.Instance.RotateToBoard();
    }
}
