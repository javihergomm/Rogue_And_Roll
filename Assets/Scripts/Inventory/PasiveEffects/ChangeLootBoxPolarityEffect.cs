using UnityEngine;

[CreateAssetMenu(menuName = "Effects/Consumables/ChangeLootBoxPolarityEffect")]
public class ChangeLootBoxPolarityEffect : BaseConsumableEffect
{
    public override void Activate(ConsumableContext ctx)
    {
        if (ctx == null || ctx.TargetSlot == null)
        {
            ctx.WasUsed = false;
            return;
        }

        BaseItemSO item = ctx.TargetSlot.ItemSO;

        // Must be a lootbox
        if (item is not LootBoxSO lootbox)
        {
            ctx.WasUsed = false;
            return;
        }

        // Flip polarity
        lootbox.ForcePolarity(
            lootbox.Type == LootBoxSO.LootType.Positive
                ? LootBoxSO.LootType.Negative
                : LootBoxSO.LootType.Positive
        );

        ctx.WasUsed = true;
    }
}
