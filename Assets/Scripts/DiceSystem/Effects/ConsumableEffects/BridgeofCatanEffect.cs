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

    // 0 = recién colocado
    // 1 = ha pasado turno enemigo
    // 2 = eliminar en el siguiente turno jugador
    private int phase = 0;

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
            Debug.LogError("SpotConnectionManager.Instance is NULL.");
            ctx.WasUsed = false;
            return;
        }

        // Registrar puente
        SpotConnectionManager.Instance.RegisterBridge(leftIndex, rightIndex);
        active = true;
        phase = 0;

        // Visual
        if (bridgePrefab != null)
        {
            Object.Instantiate(
                bridgePrefab,
                colorSpot.transform.position,
                Quaternion.identity
            );
        }

        // Registrar como efecto TEMPORAL (no pasivo)
        CharacterEffectManager.Instance.AddTemporaryEffect(this);

        ctx.WasUsed = true;
    }

    public override void OnTurnStart()
    {
        if (!active)
            return;

        if (phase == 0)
        {
            phase = 1;
            return;
        }

        if (phase == 1)
        {
            phase = 2;
            return;
        }

        RemoveBridge();
    }

    private void RemoveBridge()
    {
        if (!active)
            return;

        active = false;

        SpotConnectionManager.Instance.UnregisterBridge(leftIndex, rightIndex);

        // Eliminar efecto temporal
        CharacterEffectManager.Instance.RemoveTemporaryEffect(this);

        Debug.Log("Bridge of Catan expired after 1 round.");
    }
}
