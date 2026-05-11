using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Consumables/ChangeLootBoxPolarityEffect")]
public class ChangeLootBoxPolarityEffect : BaseConsumableEffect
{
    public override void Activate(ConsumableContext ctx)
    {
        if (ctx == null || ctx.TargetSlot == null)
            return;

        BaseItemSO item = ctx.TargetSlot.ItemSO;

        if (item is not LootBoxSO lootbox)
        {
            Debug.Log("Varita de Cambio: el objeto seleccionado no es una lootbox.");
            return;
        }

        // Invertir polaridad
        if (lootbox.Type == LootBoxSO.LootType.Positive)
            lootbox.ForcePolarity(LootBoxSO.LootType.Negative);
        else
            lootbox.ForcePolarity(LootBoxSO.LootType.Positive);

        Debug.Log("Varita de Cambio: polaridad invertida.");

        ctx.WasUsed = true;
    }
}
