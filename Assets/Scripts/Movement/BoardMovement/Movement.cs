using System;
using System.Collections;
using UnityEngine;

/*
 * Movement
 * --------
 * Controla el movimiento del jugador o enemigo sobre el tablero.
 * Utiliza la lista de casillas (spots) como puntos de destino.
 * Se desplaza paso a paso según el número obtenido en el dado.
 * Aplica efectos especiales al caer en casillas buenas o malas.
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

    [SerializeField] float speed;      // Velocidad de movimiento
    [SerializeField] int actualPos = -1; // Índice actual en el tablero
    [SerializeField] bool isPlayer;    // Indica si este movimiento pertenece al jugador

    bool EcanMove = true; // Control del turno del enemigo
    bool PcanMove = true; // Control del turno del jugador
    int Pturn = 1;
    int Eturn = 1;

    [SerializeField] AudioSource audioSource; // Sonido de movimiento
    [SerializeField] AudioClip moveSound;

    public Action OnMovementFinished; // Evento al terminar el movimiento

    private void Start()
    {
        // Busca todas las casillas del tablero
        spots = FindObjectsOfType<Spot>();

        // Ordena las casillas por su índice
        Array.Sort(spots, (a, b) => a.index.CompareTo(b.index));

        // Construye la ruta de movimiento usando los transforms de cada casilla
        positions = new Transform[spots.Length];
        for (int i = 0; i < spots.Length; i++)
            positions[i] = spots[i].transform;
    }

    /*
     * Inicia el movimiento usando el resultado del dado del jugador.
     */
    public void StartMoving()
    {
        Pturn = 1;
        Eturn = 1;

        if (isPlayer && PcanMove)
        {
            int finalRoll = InventoryManager.Instance.GetFinalDiceNumber();
            StartCoroutine(Move(finalRoll));

            if (spots[actualPos].getType() == Spot.SpotType.Good)
            {
                GoodSpotEffect();
                if (!EcanMove)
                    Pturn = 0;
            }
            else if (spots[actualPos].getType() == Spot.SpotType.Bad)
            {
                BadSpotEffect();
                if (!PcanMove)
                    Pturn = 0;
            }
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
     * Inicia el movimiento con un número fijo de pasos.
     */
    public void StartMovingFixed(int steps)
    {
        StartCoroutine(Move(steps));
    }

    /*
     * Realiza el movimiento paso a paso sobre el tablero.
     */
    private IEnumerator Move(int steps)
    {
        if (!isPlayer)
            yield return new WaitForSeconds(1f);

        for (int i = 0; i < steps; i++)
        {
            // Si se llega al final del tablero, vuelve al inicio
            if (actualPos + 1 >= positions.Length)
                actualPos = -1;

            // Avanza a la siguiente casilla
            actualPos++;

            Vector3 destination = positions[actualPos].position;
            PlayMovementSound();

            // Se mueve hacia la casilla objetivo
            while (Vector3.Distance(transform.position, destination) > 0.0000001f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    destination,
                    speed * Time.deltaTime
                );
                yield return null;
            }

            // Ajuste final de precisión
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

        OnMovementFinished?.Invoke();
    }

    /*
     * Aplica efectos al caer en una casilla buena.
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
     * Aplica efectos al caer en una casilla mala.
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
     * Propiedad para obtener o asignar la posición actual en el tablero.
     */
    public int ActualPos
    {
        get => actualPos;
        set => actualPos = value;
    }
}
