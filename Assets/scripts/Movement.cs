using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField] Transform[] positions;
    private Spot[] spots;
    [SerializeField] float speed;
    [SerializeField] int actualPos;
    [SerializeField] bool isPlayer;
    void Start()
    {
        spots = FindObjectsOfType<Spot>();
    }
    public void StartMoving()
    {
        if (isPlayer)
        {
            StartCoroutine(Move(PlayerDice.ThrowDice()));
            if(spots[actualPos + 1].getType() == Spot.SpotType.Bad)
            {
                Debug.Log("has caido en una casilla mala");
            }else if (spots[actualPos + 1].getType() == Spot.SpotType.Good){
                Debug.Log("has caido en una casilla buena");
            }
            else
            {
                Debug.Log("has caido en una casilla normal");
            }
            
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
