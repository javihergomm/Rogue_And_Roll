using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Spot[] spots;
    [SerializeField] Transform[] positions;
    [SerializeField] float speed;
    [SerializeField] int actualPos;
    [SerializeField] bool isPlayer;
    bool EcanMove = true;
    bool PcanMove = true;
    int Pturn = 1;
    int Eturn = 1;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip moveSound;


    private void Start()
    {
        spots = FindObjectsOfType<Spot>();
    }
    public void StartMoving()
    {
        Pturn = 1;
        Eturn = 1;
        if (isPlayer && PcanMove)
        {
            // El jugador usa el resultado final del sistema de dados
            int finalRoll = InventoryManager.Instance.GetFinalDiceNumber();
            StartCoroutine(Move(finalRoll));
            if (spots[actualPos].getType() == Spot.SpotType.Good)
            {
                GoodSpotEffect();
                if (!EcanMove)
                {
                    Pturn = 0;
                }
            }
            else if(spots[actualPos].getType() == Spot.SpotType.Bad)
            {
                BadSpotEffect();
                if (!PcanMove)
                {
                    Pturn = 0;
                }
            }
        }
        else if(EcanMove)
        {
            // El enemigo sigue usando su dado normal
            StartCoroutine(Move(EnemyDice.ThrowDice()));
        }

        if (Eturn == 1)
        {
            EcanMove = true;
        }
            
        if (Pturn == 1)
        {
            PcanMove = true;
        }
    }

    IEnumerator Move(int steps)
    {
        if (!isPlayer)
        {
            yield return new WaitForSeconds(1f);
        }
        for (int i = 0; i < steps; i++)
        {
            if (actualPos + 1 >= positions.Length)
                actualPos = -1;

            actualPos++;

            Vector3 destination = positions[actualPos].position;
            PlayMovementSound();
            while (Vector3.Distance(transform.position, destination) > 0.0000001f)
            {
                
                transform.position = Vector3.MoveTowards
                    (transform.position, destination, speed * Time.deltaTime);

                yield return null;
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }

    void GoodSpotEffect()
    {
        int effectType = SpotController.GoodSpot();
        //Avanza de 3 a 6 casillas
        if (effectType == 1)
        {
            Debug.Log("avnaza mas casillas");
            Move(Random.Range(3, 6));
        }
        //Tiene una tirada extra/el enemigo pierde turno
        else if (effectType == 2) 
        {
            Debug.Log("tirada extra");
            EcanMove = false;
        }
        //lootbox
        else if (effectType == 3)
        {
            Debug.Log("lootbox");
        }


    }
    void BadSpotEffect()
    {
        int effectType = SpotController.BadSpot();
        //retroceder casillas
        if (effectType == 1)
        {
            Move(Random.Range(-3, -6));
        }
        //Jugador pierde turno/enemigo tiene tirada extra
        else if (effectType == 2)
        {
            PcanMove = false;
        }
        //nada pasa
        else if (effectType == 3) 
        { 
            
        }
    }
    void PlayMovementSound()
    {
        audioSource.PlayOneShot(moveSound);
    }
}
