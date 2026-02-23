using System;
using System.Collections;
using UnityEngine;

/*
 * Movement
 * --------
 * Controla el movimiento del jugador o enemigo en el tablero.
 * Se mueve paso a paso, comprueba conexiones tipo puente,
 * aplica efectos de casilla al finalizar y permite efectos
 * temporales como ocultar la pieza del jugador.
 *
 * NOTA:
 * - El movimiento del jugador puede ser bloqueado por efectos pasivos.
 * - 
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

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip moveSound;

    public Action OnMovementFinished;

    private Renderer cachedRenderer;
    private bool wasHiddenByEffect = false;

    private void Start()
    {
        // 1. Cargar y ordenar las casillas del tablero
        spots = FindObjectsOfType<Spot>();
        Array.Sort(spots, (a, b) => a.index.CompareTo(b.index));

        // 2. Guardar las posiciones de cada casilla
        positions = new Transform[spots.Length];
        for (int i = 0; i < spots.Length; i++)
            positions[i] = spots[i].transform;

        // 3. Cachear el renderer para ocultar/mostrar la pieza
        cachedRenderer = GetComponentInChildren<Renderer>();

        // 4. Colocar la pieza en la casilla inicial (index 1..N)
        if (actualPos >= 1 && actualPos <= positions.Length)
            transform.position = positions[actualPos - 1].position;
    }

    /*
     * Inicia el movimiento usando la tirada del jugador.
     * Si el jugador tiene un efecto que bloquea el movimiento,
     * simplemente no se mueve.
     */
    public void StartMoving()
    {
        if (isPlayer && StatManager.Instance.PreventMovementThisTurn)
            return;

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
     * Actualiza la visibilidad antes de comenzar el movimiento.
     */
    private IEnumerator MoveWithVisibilityCheck(int? fixedSteps = null)
    {
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
     * Realiza el movimiento paso a paso, comprueba puentes
     * y aplica efectos de casilla.
     */
    private IEnumerator Move(int steps)
    {
        // El enemigo espera un poco antes de moverse
        if (!isPlayer)
            yield return new WaitForSeconds(1f);

        for (int i = 0; i < steps; i++)
        {
            actualPos++;

            Vector3 destino = positions[actualPos - 1].position;
            PlayMovementSound();

            // Movimiento suave hacia la casilla
            while (Vector3.Distance(transform.position, destino) > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    destino,
                    speed * Time.deltaTime
                );
                yield return null;
            }

            transform.position = destino;

            // Comprobar si hay puente desde esta casilla
            var conexiones = SpotConnectionManager.Instance.GetConnections(actualPos);
            if (conexiones.Count > 0)
            {
                int target = conexiones[0];
                actualPos = target;

                Vector3 destinoPuente = positions[target - 1].position;

                while (Vector3.Distance(transform.position, destinoPuente) > 0.0001f)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        destinoPuente,
                        speed * Time.deltaTime
                    );
                    yield return null;
                }

                transform.position = destinoPuente;
            }

            yield return new WaitForSeconds(0.1f);
        }

        // Aplicar efectos de casilla
        var tipo = spots[actualPos - 1].getType();

        if (tipo == Spot.SpotType.Good)
            GoodSpotEffect();
        else if (tipo == Spot.SpotType.Bad)
            BadSpotEffect();

        SpotConnectionManager.Instance.OnRoundStepCompleted();

        // Restaurar visibilidad si estaba oculta por un efecto
        if (isPlayer && cachedRenderer != null && wasHiddenByEffect)
        {
            cachedRenderer.enabled = true;
            wasHiddenByEffect = false;
        }

        // Resetear estado del dado
        if (isPlayer)
            DiceRollManager.Instance.ResetDiceTurnState();

        OnMovementFinished?.Invoke();
    }

    void GoodSpotEffect()
    {
        int effectType = SpotController.GoodSpot();

        if (effectType == 1)
        {
            int extra = UnityEngine.Random.Range(3, 6);
            StartCoroutine(Move(extra));
        }
    }

    void BadSpotEffect()
    {
        int effectType = SpotController.BadSpot();

        if (effectType == 1)
        {
            int extra = UnityEngine.Random.Range(-3, -6);
            StartCoroutine(Move(extra));
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
