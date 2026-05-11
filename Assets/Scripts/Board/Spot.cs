using System.Collections;
using UnityEngine;

public class Spot : MonoBehaviour
{
    public int index;
    public SpotType type;
    public bool checkpoint;

    public enum SpotType
    {
        Good,
        Bad,
        Normal
    }

    public void AssignType(SpotType newtype)
    {
        type = newtype;
    }

    public SpotType GetSpotType()
    {
        return type;
    }

    public IEnumerator TriggerSpotEffect(Movement player)
    {
        SpotController ctrl = SpotController.Instance;

        // ============================
        // CASILLA BUENA
        // ============================
        if (type == SpotType.Good)
        {
            player.effectAlreadyTriggered = true;

            bool lantern = ctrl.lanternBoostActive;

            // ¿LootBox positiva?
            if (Random.Range(0, 100) < ctrl.probGoodLootBox)
            {
                GiveLootBox(LootBoxSO.LootType.Positive);

                player.lastSpotEffectText = "¡Has encontrado una LootBox positiva!";

                if (lantern)
                    ctrl.lanternBoostActive = false;

                player.OnMovementFinished?.Invoke();
                player.SendRealMovementToUI(player.lastSpotEffectText);
                yield break;
            }

            // ¿Pasos extra o bloqueo enemigo?
            int roll = Random.Range(0, 100);

            if (roll < ctrl.probGoodExtraSteps)
            {
                int extra = Random.Range(3, 6);

                if (lantern)
                    extra *= 2;

                player.lastTotalMovement += extra;

                player.lastSpotEffectText = lantern
                    ? "+" + extra + " pasos extra (Linterna Potenciadora)"
                    : "+" + extra + " pasos extra";

                yield return player.StartCoroutine(player.ExtraMovementRoutine(extra));
            }
            else
            {
                var effect = ScriptableObject.CreateInstance<BlockEnemyMovementEffect>();

                if (lantern)
                    effect.turnsBlocked *= 2;

                effect.Activate();

                int turns = effect.turnsBlocked;

                player.lastSpotEffectText = lantern
                    ? "Bloqueo enemigo (" + turns + " turnos, Linterna Potenciadora)"
                    : "Bloqueo enemigo (" + turns + " turnos)";
            }

            if (lantern)
                ctrl.lanternBoostActive = false;

            player.OnMovementFinished?.Invoke();
            player.SendRealMovementToUI(player.lastSpotEffectText);
            yield break;
        }

        // ============================
        // CASILLA MALA
        // ============================
        if (type == SpotType.Bad)
        {
            player.effectAlreadyTriggered = true;

            // ESCUDO DE SALIDA
            if (ctrl.exitShieldActive)
            {
                ctrl.exitShieldActive = false;

                player.lastSpotEffectText =
                    "Efecto de casilla negativa anulado por Escudo de Salida";

                player.OnMovementFinished?.Invoke();
                player.SendRealMovementToUI(player.lastSpotEffectText);
                yield break;
            }

            // TRÉBOL (LootBox aleatoria)
            if (ctrl.cloverActive)
            {
                LootBoxSO box = ScriptableObject.CreateInstance<LootBoxSO>();
                box.RandomizePolarity();

                InventoryManager.Instance.AddItem(box, 1);

                player.lastSpotEffectText =
                    "El Trébol ha transformado la casilla en una LootBox aleatoria.";

                // Restaurar probabilidades
                ctrl.probBadNegativeSteps = ctrl.savedBadSteps;
                ctrl.probBadBlockPlayer = ctrl.savedBadBlock;
                ctrl.probBadLootBox = ctrl.savedBadLoot;

                ctrl.cloverActive = false;

                player.OnMovementFinished?.Invoke();
                player.SendRealMovementToUI(player.lastSpotEffectText);
                yield break;
            }

            bool doubled = StatManager.Instance.PassiveCtx.DoubleBadSpotEffects;

            // ¿LootBox negativa normal?
            if (Random.Range(0, 100) < ctrl.probBadLootBox)
            {
                GiveLootBox(LootBoxSO.LootType.Negative);

                player.lastSpotEffectText = "¡Has encontrado una LootBox negativa!";

                player.OnMovementFinished?.Invoke();
                player.SendRealMovementToUI(player.lastSpotEffectText);
                yield break;
            }

            // ¿Pasos negativos o bloqueo jugador?
            int roll = Random.Range(0, 100);

            if (roll < ctrl.probBadNegativeSteps)
            {
                int extra = Random.Range(-3, -6);

                if (doubled)
                    extra *= 2;

                player.lastTotalMovement += extra;

                player.lastSpotEffectText = doubled
                    ? extra + " pasos extra (Gafas Destruidas)"
                    : extra + " pasos extra";

                yield return player.StartCoroutine(player.ExtraMovementRoutine(extra));
            }
            else
            {
                var effect = ScriptableObject.CreateInstance<BlockPlayerMovementEffect>();

                int turns = effect.turnsBlocked;

                if (doubled)
                    turns *= 2;

                effect.turnsBlocked = turns;
                effect.Activate();

                player.lastSpotEffectText = doubled
                    ? "Bloqueo jugador (" + turns + " turnos, Gafas Destruidas)"
                    : "Bloqueo jugador (" + turns + " turnos)";
            }

            player.OnMovementFinished?.Invoke();
            player.SendRealMovementToUI(player.lastSpotEffectText);
            yield break;
        }
    }

    private void GiveLootBox(LootBoxSO.LootType polarity)
    {
        LootBoxSO box = ScriptableObject.CreateInstance<LootBoxSO>();
        box.ForcePolarity(polarity);

        InventoryManager.Instance.AddItem(box, 1);
    }
}
