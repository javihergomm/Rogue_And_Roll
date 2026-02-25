using System;
using System.Collections;
using UnityEngine;

/*
 * Movement
 * --------
 * Controls movement for player or enemy tokens on the board.
 * Moves step by step, checks bridge connections, applies spot effects,
 * and supports temporary effects such as hiding the token.
 */
public class Movement : MonoBehaviour
{
    private Spot[] spots;

    [SerializeField] private Transform[] positions;
    public Transform[] Positions => positions;
    private ShopExitManager shopExitManager;
    public void SetPositions(Transform[] newPositions)
    {
        positions = newPositions;
    }

    [SerializeField] float speed;
    [SerializeField] int actualPos = -1;
    [SerializeField] public bool isPlayer;
    bool EcanMove = true;
    bool PcanMove = true;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip moveSound;
    private int NextCheckpoint;
    public Action OnMovementFinished;

    private Renderer cachedRenderer;
    private bool wasHiddenByEffect = false;
    private int round=1;

    // Added for lap detection and enemy initialization
    public int startPos;
    public int lastPos;

    private void Start()
    {
        // Load and sort board spots
        spots = FindObjectsOfType<Spot>();
        shopExitManager = FindFirstObjectByType<ShopExitManager>();
        Array.Sort(spots, (a, b) => a.index.CompareTo(b.index));

        // Cache positions from spots
        positions = new Transform[spots.Length];
        for (int i = 0; i < spots.Length; i++)
            positions[i] = spots[i].transform;

        // Cache renderer
        cachedRenderer = GetComponentInChildren<Renderer>();

        // Place token at initial position (index 1..N)
        if (actualPos >= 1 && actualPos <= positions.Length)
            transform.position = positions[actualPos - 1].position;

        // Initialize lap tracking and enemy movement state
        startPos = actualPos;
        lastPos = actualPos;
    }

    /*
     * Starts movement using the player's dice roll.
     */
    public void StartMoving()
    {
        NextCheckpoint = GetNextCheckpoint();
        if (isPlayer && StatManager.Instance.PreventMovementThisTurn)
            return;

        StartCoroutine(MoveWithVisibilityCheck());
    }

    /*
     * Starts movement with a fixed number of steps.
     */
    public void StartMovingFixed(int steps)
    {

        StartCoroutine(MoveWithVisibilityCheck(steps));
    }

    /*
     * Updates visibility before movement.
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
        int divisor = 1;

        if (round >= 3)
        {
            divisor = ((round - 3) / 2) + 2;
        }
        if (isPlayer)
        {
            steps = steps / divisor;
        }
        
        yield return StartCoroutine(Move(steps));
    }

    /*
     * Performs movement step by step, checks bridges,
     * and applies spot effects.
     */
    private IEnumerator Move(int steps)
    {
        int hipoteticSpot = actualPos + steps;
        if(hipoteticSpot > NextCheckpoint)
        {
            steps = NextCheckpoint - actualPos;
        }

        if (!isPlayer)
            yield return new WaitForSeconds(1f);

        for (int i = 0; i < steps; i++)
        {
            actualPos++;

            // Wrap-around: if we pass the last spot, return to 1
            if (actualPos > positions.Length)
                actualPos = 1;

            Vector3 destino = positions[actualPos - 1].position;
            PlayMovementSound();

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

            // Bridge connections
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

        var tipo = spots[actualPos - 1].getType();
        if (isPlayer)
        {
            if (spots[actualPos-1].checkpoint == true)
            {
                shopExitManager.EnterShop();
                round++;
            }
            else if (tipo == Spot.SpotType.Good)
                GoodSpotEffect();
            else if (tipo == Spot.SpotType.Bad)
                BadSpotEffect();
        }
        

        SpotConnectionManager.Instance.OnRoundStepCompleted();

        if (isPlayer && cachedRenderer != null && wasHiddenByEffect)
        {
            cachedRenderer.enabled = true;
            wasHiddenByEffect = false;
        }

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

            Debug.Log((isPlayer ? "Player" : "Enemy") +
                      " received GOOD effect: extra steps = " + extra);

            StartCoroutine(Move(extra));
        }
        else if (effectType == 2)
        {
            EcanMove = false;

            Debug.Log((isPlayer ? "Player" : "Enemy") +
                      " received GOOD effect: enemy cannot move next turn");
        }
        else if (effectType == 3)
        {
            Debug.Log((isPlayer ? "Player" : "Enemy") +
                      " received GOOD effect: lootbox");
            // Lootbox logic here
        }
    }

    void BadSpotEffect()
    {
        int effectType = SpotController.BadSpot();

        if (effectType == 1)
        {
            int extra = UnityEngine.Random.Range(-3, -6);

            Debug.Log((isPlayer ? "Player" : "Enemy") +
                      " received BAD effect: extra steps = " + extra);

            StartCoroutine(Move(extra));
        }
        else if (effectType == 2)
        {
            PcanMove = false;

            Debug.Log((isPlayer ? "Player" : "Enemy") +
                      " received BAD effect: player cannot move next turn");
        }
        else if (effectType == 3)
        {
            Debug.Log((isPlayer ? "Player" : "Enemy") +
                      " received BAD effect: other negative effect");
            // Other negative effect
        }
    }

    public int GetNextCheckpoint()
    {
        int total = spots.Length;

        for (int i = 1; i <= total; i++)
        {
            int nextPos = ((actualPos - 1 + i) % total);

            if (spots[nextPos].checkpoint)
            {
                return spots[nextPos].index;
            }
        }

        return -1;
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
