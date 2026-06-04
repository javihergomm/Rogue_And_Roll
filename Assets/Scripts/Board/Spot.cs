using System.Collections;
using UnityEngine;

/*
 * Spot
 * ----
 * Represents a board tile that can trigger Good or Bad effects.
 */
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

        // ============================================================
        // GOOD SPOT
        // ============================================================
        if (type == SpotType.Good)
        {
            player.effectAlreadyTriggered = true;

            bool lantern = ctrl.lanternBoostActive;

            // Player receives gold silently (no message)
            StatManager.Instance.ChangeStat(StatType.Gold, 5);

            // LootBox positive?
            if (Random.Range(0, 100) < ctrl.probGoodLootBox)
            {
                GiveLootBox(LootBoxSO.LootType.Positive);

                player.lastSpotEffectText = "Has encontrado una LootBox.";

                if (lantern)
                    ctrl.lanternBoostActive = false;

                player.OnMovementFinished?.Invoke();
                player.SendRealMovementToUI(player.lastSpotEffectText);
                yield break;
            }

            // Extra steps or enemy block
            int roll = Random.Range(0, 100);

            if (roll < ctrl.probGoodExtraSteps)
            {
                int extra = Random.Range(3, 6);

                if (lantern)
                    extra *= 2;

                player.lastTotalMovement += extra;

                if (lantern)
                    player.lastSpotEffectText = "+" + extra + " pasos extra (Linterna Potenciadora)";
                else
                    player.lastSpotEffectText = "+" + extra + " pasos extra";

                yield return player.StartCoroutine(player.ExtraMovementRoutine(extra));
            }
            else
            {
                var effect = ScriptableObject.CreateInstance<BlockEnemyMovementEffect>();

                if (lantern)
                    effect.turnsBlocked *= 2;

                effect.Activate();

                int turns = effect.turnsBlocked;

                if (lantern)
                    player.lastSpotEffectText = "Bloqueo enemigo (" + turns + " turnos, Linterna Potenciadora)";
                else
                    player.lastSpotEffectText = "Bloqueo enemigo (" + turns + " turnos)";
            }

            if (lantern)
                ctrl.lanternBoostActive = false;

            player.OnMovementFinished?.Invoke();
            player.SendRealMovementToUI(player.lastSpotEffectText);
            yield break;
        }

        // ============================================================
        // BAD SPOT
        // ============================================================
        if (type == SpotType.Bad)
        {
            player.effectAlreadyTriggered = true;

            // EXIT SHIELD
            if (ctrl.exitShieldActive)
            {
                ctrl.exitShieldActive = false;

                player.lastSpotEffectText =
                    "Efecto negativo anulado por Escudo de Salida";

                player.OnMovementFinished?.Invoke();
                player.SendRealMovementToUI(player.lastSpotEffectText);
                yield break;
            }

            // CLOVER (random LootBox)
            if (ctrl.cloverActive)
            {
                GiveRandomLootBox();

                player.lastSpotEffectText =
                    "El Trébol ha transformado la casilla en una LootBox aleatoria.";

                // Restore probabilities
                ctrl.probBadNegativeSteps = ctrl.savedBadSteps;
                ctrl.probBadBlockPlayer = ctrl.savedBadBlock;
                ctrl.probBadLootBox = ctrl.savedBadLoot;

                ctrl.cloverActive = false;

                player.OnMovementFinished?.Invoke();
                player.SendRealMovementToUI(player.lastSpotEffectText);
                yield break;
            }

            bool doubled = StatManager.Instance.PassiveCtx.DoubleBadSpotEffects;

            // Negative LootBox?
            if (Random.Range(0, 100) < ctrl.probBadLootBox)
            {
                GiveLootBox(LootBoxSO.LootType.Negative);

                player.lastSpotEffectText = "Has encontrado una LootBox.";

                player.OnMovementFinished?.Invoke();
                player.SendRealMovementToUI(player.lastSpotEffectText);
                yield break;
            }

            // Negative steps or block player
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

    // ============================================================
    // LOOTBOX HELPERS (FIXED)
    // ============================================================

    private void GiveLootBox(LootBoxSO.LootType polarity)
    {
        string path = polarity == LootBoxSO.LootType.Positive
            ? "Items/LootBox/LootBox_Positive"
            : "Items/LootBox/LootBox_Negative";

        LootBoxSO template = Resources.Load<LootBoxSO>(path);
        LootBoxSO box = Instantiate(template);

        InventoryManager.Instance.AddItem(box, 1);
    }

    private void GiveRandomLootBox()
    {
        string path = Random.Range(0, 2) == 0
            ? "Items/LootBox/LootBox_Positive"
            : "Items/LootBox/LootBox_Negative";

        LootBoxSO template = Resources.Load<LootBoxSO>(path);
        LootBoxSO box = Instantiate(template);

        InventoryManager.Instance.AddItem(box, 1);
    }
}
