using UnityEngine;

/*
 * ConsumableSO
 * ------------
 * ScriptableObject that represents a consumable item.
 * Stores the effects that the item applies when used.
 * When the item is consumed, all its effects are executed.
 */
[CreateAssetMenu(fileName = "NewConsumable", menuName = "Inventory/Consumable")]
public class ConsumableSO : BaseItemSO
{
    // List of effects that this consumable applies
    [Header("Effects")]
    [SerializeField] private BaseEffect[] effects;

    public BaseEffect[] Effects => effects;

    /*
     * Executes all effects assigned to this consumable.
     * Each effect type handles its own behavior.
     */
    public override void UseItem()
    {
        if (effects == null || effects.Length == 0)
            return;

        ConsumableContext ctx = new ConsumableContext();

        foreach (var eff in effects)
        {
            if (eff == null)
                continue;

            // Dice-related effects
            if (eff is BaseDiceEffect diceEff)
            {
                StatManager.Instance.ActiveConsumableEffects.Add(diceEff);
                continue;
            }

            // Consumable effects that run immediately
            if (eff is BaseConsumableEffect consEff)
            {
                consEff.Activate(ctx);
                continue;
            }

            // Passive effects that trigger automatically
            if (eff is BasePassiveEffect passiveEff)
            {
                passiveEff.OnTurnStart(new PassiveContext());
                continue;
            }
        }
    }
}
