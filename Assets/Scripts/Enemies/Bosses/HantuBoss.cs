using UnityEngine;

/*
 * HantuBoss
 * ---------
 * Enemy that rolls 1D6 each turn and has a chance to add +2.
 */
public class HantuBoss : EnemyBase
{
    [Header("Hantu Settings")]
    [Range(0f, 1f)]
    public float chanceToAddTwo = 0.25f;

    private void Update()
    {
        if (!isActive || movement == null || playerMovement == null)
            return;

        if (movement.ActualPos == playerMovement.ActualPos)
        {
            Debug.Log("[Hantu] Landed on player tile. Killing player.");
            KillPlayerNow();
        }
    }

    public override void SpawnEnemy()
    {
        base.SpawnEnemy();
    }

    public void ActivateHantu()
    {
        SpawnEnemy();
    }

    public override void StartTurn()
    {
        if (!isActive || movement == null)
            return;

        if (IsEnemyMovementBlocked())
        {
            Debug.Log("[Hantu] Movement blocked this turn.");
            TurnManager.Instance.ForceEnemyTurnEnd();
            return;
        }

        int roll = EnemyDice.ThrowDice();

        bool addedTwo = false;
        if (Random.value <= chanceToAddTwo)
        {
            roll += 2;
            addedTwo = true;
        }

        Debug.Log("[Hantu] Base roll: " + (roll - (addedTwo ? 2 : 0)) +
                  (addedTwo ? " (+2 bonus)" : "") +
                  " => Final: " + roll);

        TurnManager.NotifyEnemyRoll(roll);

        movement.OnMovementFinished += OnMovementFinished;
        movement.StartMovingFixed(roll);
    }



    private void OnMovementFinished()
    {
        movement.OnMovementFinished -= OnMovementFinished;
        Debug.Log("[Hantu] Movement finished. Enemy turn ends.");
        TurnManager.Instance.ForceEnemyTurnEnd();
    }
}
