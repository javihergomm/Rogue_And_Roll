using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] Transform[] positions;
    [SerializeField] float speed;
    [SerializeField] int actualPos;
    [SerializeField] bool isPlayer;
    
    public void StartMoving()
    {
        if (isPlayer)
        {
            StartCoroutine(Move(PlayerDice.ThrowDice()));
        }
        else
        {
            StartCoroutine(Move(EnemyDice.ThrowDice()));
        }

            

    }

    IEnumerator Move(int steps)
    {
        if(!isPlayer){
            yield return new WaitForSeconds(1f);
        }
        for (int i = 0; i < steps; i++)
        {
            if (actualPos + 1 >= positions.Length)
                actualPos=-1;

            actualPos++;

            Vector3 destination = positions[actualPos].position;

            while (Vector3.Distance(transform.position, destination) > 0.0000001f)
            {
                transform.position = Vector3.MoveTowards
                    (transform.position, destination, speed * Time.deltaTime);

                yield return null;
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
}
