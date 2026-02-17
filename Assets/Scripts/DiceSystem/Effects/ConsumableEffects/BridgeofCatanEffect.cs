using UnityEngine;

/*
 * BridgeOfCatanEffect
 * -------------------
 * Creates a temporary shortcut between two board positions,
 * but only when used on a ColorSpot.
 */
[CreateAssetMenu(
    fileName = "BridgeOfCatanEffect",
    menuName = "Effects/Consumables/BridgeOfCatan"
)]
public class BridgeOfCatanEffect : BaseConsumableEffect
{
    [SerializeField] private GameObject bridgePrefab;

    public override void Activate(ConsumableContext ctx)
    {
        if (ctx == null)
            return;

        // Must be used on a ColorSpot
        ColorSpot colorSpot = ctx.TargetColorSpot;
        if (colorSpot == null)
        {
            Debug.Log("Bridge of Catan can only be placed on a ColorSpot.");
            ctx.WasUsed = false;
            return;
        }

        int left = colorSpot.LeftPositionIndex;
        int right = colorSpot.RightPositionIndex;

        // Register bridge
        if (SpotConnectionManager.Instance == null)
        {
            Debug.LogError("SpotConnectionManager.Instance is NULL. Add it to the scene.");
            ctx.WasUsed = false;
            return;
        }

        SpotConnectionManager.Instance.RegisterBridge(left, right);

        // Instantiate visual prefab
        if (bridgePrefab != null)
        {
            Object.Instantiate(
                bridgePrefab,
                colorSpot.transform.position,
                Quaternion.identity
            );
        }

        ctx.WasUsed = true;
    }

}
