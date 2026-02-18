using System;
using System.Collections;
using UnityEngine;

/*
 * Movement
 * --------
 * Controla el movimiento del jugador o enemigo en el tablero.
 * Se mueve paso a paso, comprueba conexiones tipo puente,
 * aplica efectos de casilla al finalizar y permite efectos temporales
 * como ocultar la pieza del jugador (por ejemplo, Broken Map).
 */
public class Movement : MonoBehaviour
{
    private Spot[] spots;

    [SerializeField] private Transform[] positions;
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

    private Renderer cachedRenderer;
    private bool wasHiddenByEffect = false;
    private ShopExitManager shopExitManager;

    private void Start()
    {
        // Obtiene todas las casillas del tablero y las ordena por índice
        spots = FindObjectsOfType<Spot>();
        Array.Sort(spots, (a, b) => a.index.CompareTo(b.index));

        // Guarda las posiciones de cada casilla
        positions = new Transform[spots.Length];
        for (int i = 0; i < spots.Length; i++)
            positions[i] = spots[i].transform;

        // Cachea el renderer para poder ocultar/mostrar la pieza
        cachedRenderer = GetComponentInChildren<Renderer>();
    }

    /*
     * Inicia el movimiento usando la tirada del jugador.
     * Se usa una corrutina para asegurarse de que los efectos
     * (como ocultar la pieza) ya están aplicados.
     */
    public void StartMoving()
    {
        Pturn = 1;
        Eturn = 1;

        StartCoroutine(MoveWithVisibilityCheck());
    }

    /*
     * Inicia un movimiento con un número fijo de pasos.
     */
    public void StartMovingFixed(int steps)
    {
        StartCoroutine(MoveWithVisibilityCheck(steps));
    }

    /*
     * Asegura que la visibilidad de la pieza se actualiza
     * DESPUÉS de que la tirada final esté calculada.
     * Esto garantiza que efectos como Broken Map funcionen siempre.
     */
    private IEnumerator MoveWithVisibilityCheck(int? fixedSteps = null)
    {
        // Espera un frame para que StatManager actualice los flags
        yield return null;

        if (isPlayer && cachedRenderer != null)
        {
            if (StatManager.Instance.HidePieceThisTurn)
            {
                cachedRenderer.enabled = false;
                wasHiddenByEffect = true;
            }
            else
            {
                cachedRenderer.enabled = true;
                wasHiddenByEffect = false;
            }
        }

        int steps = fixedSteps ?? InventoryManager.Instance.GetFinalDiceNumber();
        yield return StartCoroutine(Move(steps));
    }

    /*
     * Realiza el movimiento paso a paso.
     * Comprueba puentes y aplica efectos de casilla.
     */
    private IEnumerator Move(int steps)
    {
        // Los enemigos esperan un poco antes de moverse
        if (!isPlayer)
            yield return new WaitForSeconds(1f);

        for (int i = 0; i < steps; i++)
        {
            // Avanza a la siguiente casilla
            if (actualPos + 1 >= positions.Length)
                actualPos = -1;

            actualPos++;

            Vector3 destination = positions[actualPos].position;
            PlayMovementSound();

            // Movimiento suave hacia la casilla
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

            // Comprueba si hay un puente desde esta casilla
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

        // Aplica efectos de casilla
        if (spots[actualPos].checkpoint == true)
        {
            shopExitManager.EnterShop();
        }
        else if (spots[actualPos].getType() == Spot.SpotType.Good)
        {
            GoodSpotEffect();
        }
        else if (spots[actualPos].getType() == Spot.SpotType.Bad)
        {
            BadSpotEffect();
        }

        // Avanza el sistema de puentes
        SpotConnectionManager.Instance.OnRoundStepCompleted();

        // Restaura la visibilidad si fue ocultada por un efecto
        if (isPlayer && cachedRenderer != null && wasHiddenByEffect)
        {
            cachedRenderer.enabled = true;
            wasHiddenByEffect = false;
        }

        OnMovementFinished?.Invoke();
    }

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

    void PlayMovementSound()
    {
        audioSource.PlayOneShot(moveSound);
    }

    public int ActualPos
    {
        get => actualPos;
        set => actualPos = value;
    }
}
