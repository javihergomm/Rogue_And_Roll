using UnityEngine;

/*
 * BridgeOfCatanEffect
 * -------------------
 * Creates a temporary shortcut between two board positions,
 * but only when used on a ColorSpot.
 *
 * Now lasts exactly 1 full round:
 * - Player turn (when placed)
 * - Enemy turn (if any)
 * - Removed automatically at the next Player turn
 */
[CreateAssetMenu(
    fileName = "BridgeOfCatanEffect",
    menuName = "Effects/Consumables/BridgeOfCatan"
)]
public class BridgeOfCatanEffect : BaseConsumableEffect
{
    [SerializeField] private GameObject bridgePrefab;

    // Internal state
    private bool active = false;
    private int leftIndex;
    private int rightIndex;

    private bool playerTurnPassed = false;
    private bool enemyTurnPassed = false;

    public override void Activate(ConsumableContext ctx)
    {
        if (ctx == null)
            return;

        ColorSpot colorSpot = ctx.TargetColorSpot;
        if (colorSpot == null)
        {
            Debug.Log("Bridge of Catan can only be placed on a ColorSpot.");
            ctx.WasUsed = false;
            return;
        }

        leftIndex = colorSpot.LeftPositionIndex;
        rightIndex = colorSpot.RightPositionIndex;

        if (SpotConnectionManager.Instance == null)
        {
            Debug.LogError("SpotConnectionManager.Instance is NULL. Add it to the scene.");
            ctx.WasUsed = false;
            return;
        }

        // Register bridge
        SpotConnectionManager.Instance.RegisterBridge(leftIndex, rightIndex);
        active = true;

        // Visual
        if (bridgePrefab != null)
        {
            Object.Instantiate(
                bridgePrefab,
                colorSpot.transform.position,
                Quaternion.identity
            );
        }

        // Subscribe to turn events
        TurnManager.OnPlayerTurnStarted -= OnPlayerTurnStarted;
        TurnManager.OnPlayerTurnStarted += OnPlayerTurnStarted;

        TurnManager.OnEnemyTurnStarted -= OnEnemyTurnStarted;
        TurnManager.OnEnemyTurnStarted += OnEnemyTurnStarted;

        ctx.WasUsed = true;
    }

    private void OnPlayerTurnStarted()
    {
        if (!active)
            return;

        // If both turns already passed -> remove bridge
        if (playerTurnPassed && enemyTurnPassed)
        {
            RemoveBridge();
            return;
        }

        // First time player turn is detected after placement
        if (!playerTurnPassed)
            playerTurnPassed = true;
    }

    private void OnEnemyTurnStarted()
    {
        if (!active)
            return;

        if (playerTurnPassed)
            enemyTurnPassed = true;
    }

    private void RemoveBridge()
    {
        active = false;

        SpotConnectionManager.Instance.UnregisterBridge(leftIndex, rightIndex);

        TurnManager.OnPlayerTurnStarted -= OnPlayerTurnStarted;
        TurnManager.OnEnemyTurnStarted -= OnEnemyTurnStarted;

        playerTurnPassed = false;
        enemyTurnPassed = false;

        Debug.Log("Bridge of Catan expired after 1 round.");
    }
}
