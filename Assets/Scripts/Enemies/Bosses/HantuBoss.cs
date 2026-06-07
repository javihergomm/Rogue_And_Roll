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

        TurnManager.NotifyEnemyRoll(roll);

        movement.OnMovementFinished += OnMovementFinished;
        movement.StartMovingFixed(roll);
    }



    private void OnMovementFinished()
    {
        movement.OnMovementFinished -= OnMovementFinished;
        TurnManager.Instance.ForceEnemyTurnEnd();
    }
}
