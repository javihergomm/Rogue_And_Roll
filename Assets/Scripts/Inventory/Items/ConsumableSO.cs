using UnityEngine;

/*
 * ConsumableSO
 * ------------
 * ScriptableObject representing a consumable item.
 * Executes its effects using a ConsumableContext.
 */
[CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable")]
public class ConsumableSO : BaseItemSO
{
    public bool CanBeUsedOnSpot => canBeUsedOnSpot;
    public bool AppearsIn3D;

    [SerializeField] private bool canBeUsedOnSpot = false;

    [SerializeField] private BaseEffect[] effects;
    public BaseEffect[] Effects => effects;
    /*
     * Required by BaseItemSO.
     * Creates a default context and executes the effects.
     */
    public override void UseItem()
    {
        UseItem(new ConsumableContext());
    }

    /*
     * Executes all effects assigned to this consumable.
     * Uses the provided ConsumableContext.
     */
    public void UseItem(ConsumableContext ctx)
    {
        if (effects == null || effects.Length == 0)
            return;

        foreach (var eff in effects)
        {
            if (eff == null)
                continue;

            if (eff is BaseDiceEffect diceEff)
            {
                StatManager.Instance.ActiveConsumableEffects.Add(diceEff);
                continue;
            }

            if (eff is BaseConsumableEffect consEff)
            {
                consEff.Activate(ctx);
                continue;
            }

            if (eff is BasePassiveEffect passiveEff)
            {
                passiveEff.OnTurnStart(new PassiveContext());
                continue;
            }
        }
    }
}
