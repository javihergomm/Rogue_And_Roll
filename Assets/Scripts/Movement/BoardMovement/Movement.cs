using System;
using System.Collections;
using UnityEngine;

/*
 * Movement
 * --------
 * Controla el movimiento del jugador o enemigo sobre el tablero.
 * Avanza paso a paso, comprobando si hay un puente en cada casilla.
 * Si pisa un puente, lo cruza inmediatamente y sigue moviéndose desde ahi.
 * Los efectos de casilla (buena o mala) se aplican solo al final del movimiento.
 */
public class Movement : MonoBehaviour
{
    private Spot[] spots; // Lista de casillas del tablero

    [SerializeField] private Transform[] positions; // Transform de cada casilla
    public Transform[] Positions => positions;

    public void SetPositions(Transform[] newPositions)
    {
        positions = newPositions;
    }

    [SerializeField] float speed;
    [SerializeField] int actualPos = -1;
    [SerializeField] bool isPlayer;

    bool EcanMove = true;
    bool PcanMove = true;
    int Pturn = 1;
    int Eturn = 1;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip moveSound;

    public Action OnMovementFinished;

    private void Start()
    {
        spots = FindObjectsOfType<Spot>();
        Array.Sort(spots, (a, b) => a.index.CompareTo(b.index));

        positions = new Transform[spots.Length];
        for (int i = 0; i < spots.Length; i++)
            positions[i] = spots[i].transform;
    }

    /*
     * Inicia el movimiento usando el resultado del dado.
     * Los efectos de casilla ya no se aplican aqui.
     */
    public void StartMoving()
    {
        Pturn = 1;
        Eturn = 1;

        if (isPlayer && PcanMove)
        {
            int finalRoll = InventoryManager.Instance.GetFinalDiceNumber();
            StartCoroutine(Move(finalRoll));
        }
        else if (EcanMove)
        {
            StartCoroutine(Move(EnemyDice.ThrowDice()));
        }

        if (Eturn == 1)
            EcanMove = true;

        if (Pturn == 1)
            PcanMove = true;
    }

    /*
     * Inicia el movimiento con un numero fijo de pasos.
     */
    public void StartMovingFixed(int steps)
    {
        StartCoroutine(Move(steps));
    }

    /*
     * Movimiento paso a paso.
     * Comprueba puentes en cada casilla.
     * Aplica efectos solo al final.
     */
    private IEnumerator Move(int steps)
    {
        if (!isPlayer)
            yield return new WaitForSeconds(1f);

        for (int i = 0; i < steps; i++)
        {
            // Avanzar una casilla
            if (actualPos + 1 >= positions.Length)
                actualPos = -1;

            actualPos++;

            Vector3 destination = positions[actualPos].position;
            PlayMovementSound();

            while (Vector3.Distance(transform.position, destination) > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    destination,
                    speed * Time.deltaTime
                );
                yield return null;
            }

            transform.position = destination;

            // Comprobar puente en esta casilla
            var connections = SpotConnectionManager.Instance.GetConnections(actualPos);
            if (connections.Count > 0)
            {
                int target = connections[0];
                actualPos = target;

                Vector3 destBridge = positions[target].position;

                while (Vector3.Distance(transform.position, destBridge) > 0.0001f)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        destBridge,
                        speed * Time.deltaTime
                    );
                    yield return null;
                }

                transform.position = destBridge;
            }

            yield return new WaitForSeconds(0.1f);
        }

        /*
         * Aplicar efectos de la casilla final.
         */
        if (spots[actualPos].getType() == Spot.SpotType.Good)
        {
            GoodSpotEffect();
        }
        else if (spots[actualPos].getType() == Spot.SpotType.Bad)
        {
            BadSpotEffect();
        }

        /*
         * Notificar fin de turno para actualizar puentes.
         */
        SpotConnectionManager.Instance.OnRoundStepCompleted();

        OnMovementFinished?.Invoke();
    }

    /*
     * Efectos de casilla buena.
     */
    void GoodSpotEffect()
    {
        int effectType = SpotController.GoodSpot();

        if (effectType == 1)
        {
            StartCoroutine(Move(UnityEngine.Random.Range(3, 6)));
        }
        else if (effectType == 2)
        {
            EcanMove = false;
        }
        else if (effectType == 3)
        {
            // Lootbox
        }
    }

    /*
     * Efectos de casilla mala.
     */
    void BadSpotEffect()
    {
        int effectType = SpotController.BadSpot();

        if (effectType == 1)
        {
            StartCoroutine(Move(UnityEngine.Random.Range(-3, -6)));
        }
        else if (effectType == 2)
        {
            PcanMove = false;
        }
        else if (effectType == 3)
        {
            // Otro efecto negativo
        }
    }

    /*
     * Reproduce el sonido de movimiento.
     */
    void PlayMovementSound()
    {
        audioSource.PlayOneShot(moveSound);
    }

    /*
     * Propiedad para obtener o asignar la posicion actual.
     */
    public int ActualPos
    {
        get => actualPos;
        set => actualPos = value;
    }
}
