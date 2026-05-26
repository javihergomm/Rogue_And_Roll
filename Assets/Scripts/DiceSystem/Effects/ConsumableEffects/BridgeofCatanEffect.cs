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

    // 0 = recien colocado
    // 1 = ha pasado turno enemigo (o turno jugador si no hay enemigos)
    // 2 = eliminar en el siguiente turno jugador
    private int phase = 0;

    // Referencia global al ultimo puente visual instanciado
    public static GameObject lastBridgeVisual;

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

        // Registrar puente (bidireccional si tu manager ya lo soporta)
        SpotConnectionManager.Instance.RegisterBridge(leftIndex, rightIndex);
        active = true;
        phase = 0;

        // Instanciar visual con rotacion segun tipo de ColorSpot
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

        // Registrar como efecto temporal
        CharacterEffectManager.Instance.AddTemporaryEffect(this);

        ctx.WasUsed = true;
    }

    public override void OnTurnStart()
    {
        if (!active)
            return;

        // Fase 0 -> recien colocado
        if (phase == 0)
        {
            phase = 1;
            return;
        }

        // Fase 1 -> turno enemigo o turno jugador si no hay enemigos
        if (phase == 1)
        {
            phase = 2;
            return;
        }

        // Fase 2 -> eliminar
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

        // Eliminar visual si existe
        if (lastBridgeVisual != null)
        {
            Object.Destroy(lastBridgeVisual);
            lastBridgeVisual = null;
        }

        Debug.Log("Bridge of Catan expired after 1 round.");
    }

    // ---------------------------------------------------------
    // Metodos para ocultar/mostrar el puente visual
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
