using System;
using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] public Transform[] positions;
    [SerializeField] private float speed;
    [SerializeField] private int actualPos;
    [SerializeField] private bool isPlayer;

    public Action OnMovementFinished;

    public void StartMoving()
    {
        if (isPlayer)
        {
            int finalRoll = InventoryManager.Instance.GetFinalDiceNumber();
            StartCoroutine(Move(finalRoll));
        }
        else
        {
            StartCoroutine(Move(EnemyDice.ThrowDice()));
        }
    }

    // Used by DemonBoss to move with a fixed number of steps
    public void StartMovingFixed(int steps)
    {
        StartCoroutine(Move(steps));
    }

    private IEnumerator Move(int steps)
    {
        if (!isPlayer)
            yield return new WaitForSeconds(1f);

        for (int i = 0; i < steps; i++)
        {
            if (actualPos + 1 >= positions.Length)
                actualPos = -1;

            actualPos++;

            Vector3 destination = positions[actualPos].position;

            while (Vector3.Distance(transform.position, destination) > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    destination,
                    speed * Time.deltaTime
                );

                yield return null;
            }

            yield return new WaitForSeconds(0.1f);
        }

        if (OnMovementFinished != null)
            OnMovementFinished.Invoke();
    }

    public int ActualPos
    {
        get => actualPos;
        set => actualPos = value;
    }

}
