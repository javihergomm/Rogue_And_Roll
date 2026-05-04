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
        if (type == SpotType.Good)
        {
            player.effectAlreadyTriggered = true;

            int roll = Random.Range(0, 100);

            if (roll < player.probabilityExtraSteps)
            {
                int extra = Random.Range(3, 6);
                player.lastTotalMovement += extra;

                player.lastSpotEffectText = "+" + extra + " pasos extra";

                yield return player.StartCoroutine(player.ExtraMovementRoutine(extra));
            }
            else
            {
                var effect = ScriptableObject.CreateInstance<BlockEnemyMovementEffect>();
                effect.Activate();

                int turns = effect.turnsBlocked;
                player.lastSpotEffectText = "Bloqueo enemigo (" + turns + " turno" + (turns > 1 ? "s" : "") + ")";
            }

            player.OnMovementFinished?.Invoke();
            player.SendRealMovementToUI(player.lastSpotEffectText);
            yield break;
        }

        if (type == SpotType.Bad)
        {
            player.effectAlreadyTriggered = true;

            int roll = Random.Range(0, 100);

            if (roll < player.probabilityNegativeSteps)
            {
                int extra = Random.Range(-3, -6);

                if (StatManager.Instance.PassiveCtx.DoubleBadSpotEffects)
                    extra *= 2;

                player.lastTotalMovement += extra;

                player.lastSpotEffectText = extra + " pasos extra";

                yield return player.StartCoroutine(player.ExtraMovementRoutine(extra));
            }
            else
            {
                var effect = ScriptableObject.CreateInstance<BlockPlayerMovementEffect>();

                int turns = effect.turnsBlocked;

                if (StatManager.Instance.PassiveCtx.DoubleBadSpotEffects)
                    turns *= 2;

                effect.turnsBlocked = turns;
                effect.Activate();

                player.lastSpotEffectText = "Bloqueo jugador (" + turns + " turno" + (turns > 1 ? "s" : "") + ")";
            }

            player.OnMovementFinished?.Invoke();
            player.SendRealMovementToUI(player.lastSpotEffectText);
            yield break;
        }
    }
}
