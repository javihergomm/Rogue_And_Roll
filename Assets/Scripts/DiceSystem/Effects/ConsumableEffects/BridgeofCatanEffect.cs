using UnityEngine;

[CreateAssetMenu(
    fileName = "BridgeOfCatanEffect",
    menuName = "Effects/Consumables/BridgeOfCatan"
)]
public class BridgeOfCatanEffect : BaseConsumableEffect
{
    [SerializeField] private GameObject bridgePrefab;

    private int leftIndex;
    private int rightIndex;
    private bool active = false;

    // 0 = just placed
    // 1 = enemy turn passed (or player turn if no enemies)
    // 2 = remove on next player turn
    private int phase = 0;

    // Global reference to the last instantiated bridge visual
    public static GameObject lastBridgeVisual;

    /*
     * Activate
     * --------
     * Places the bridge between two ColorSpot indices, registers the
     * connection, spawns the visual, and marks the effect as temporary.
     */
    public override void Activate(ConsumableContext ctx)
    {
        if (ctx == null)
            return;

        ColorSpot colorSpot = ctx.TargetColorSpot;
        if (colorSpot == null)
        {
            ctx.WasUsed = false;
            return;
        }

        leftIndex = colorSpot.LeftPositionIndex;
        rightIndex = colorSpot.RightPositionIndex;

        if (SpotConnectionManager.Instance == null)
        {
            ctx.WasUsed = false;
            return;
        }

        // Register the bridge connection
        SpotConnectionManager.Instance.RegisterBridge(leftIndex, rightIndex);
        active = true;
        phase = 0;

        // Spawn the visual with rotation depending on the ColorSpot type
        if (bridgePrefab != null)
        {
            Quaternion rot = Quaternion.identity;

            string spotName = colorSpot.name;
            if (spotName.Contains("RedSpot") || spotName.Contains("YellowSpot"))
            {
                rot = Quaternion.Euler(0f, 90f, 0f);
            }

            var instance = Object.Instantiate(
                bridgePrefab,
                colorSpot.transform.position,
                rot
            );

            lastBridgeVisual = instance;
        }

        // Register as a temporary effect
        CharacterEffectManager.Instance.AddTemporaryEffect(this);

        ctx.WasUsed = true;
    }

    /*
     * OnTurnStart
     * -----------
     * Handles the lifetime of the bridge across turns.
     */
    public override void OnTurnStart()
    {
        if (!active)
            return;

        // Phase 0 -> just placed
        if (phase == 0)
        {
            phase = 1;
            return;
        }

        // Phase 1 -> enemy turn passed (or player turn if no enemies)
        if (phase == 1)
        {
            phase = 2;
            return;
        }

        // Phase 2 -> remove the bridge
        RemoveBridge();
    }

    /*
     * RemoveBridge
     * ------------
     * Unregisters the connection, removes the temporary effect,
     * destroys the visual, and clears state.
     */
    private void RemoveBridge()
    {
        if (!active)
            return;

        active = false;

        SpotConnectionManager.Instance.UnregisterBridge(leftIndex, rightIndex);

        // Remove temporary effect
        CharacterEffectManager.Instance.RemoveTemporaryEffect(this);

        // Destroy the visual if it exists
        if (lastBridgeVisual != null)
        {
            Object.Destroy(lastBridgeVisual);
            lastBridgeVisual = null;
        }

    }

    // ---------------------------------------------------------
    // Methods to hide or show the bridge visual
    // ---------------------------------------------------------

    public static void HideVisual()
    {
        if (lastBridgeVisual != null)
            lastBridgeVisual.SetActive(false);
    }

    public static void ShowVisual()
    {
        if (lastBridgeVisual != null)
            lastBridgeVisual.SetActive(true);
    }
}
