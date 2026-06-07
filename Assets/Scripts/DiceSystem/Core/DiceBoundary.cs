using UnityEngine;

/*
 * DiceBoundary
 * ------------
 * Keeps the dice inside a defined 3D area.
 * If the dice leaves the allowed region, it is respawned at its spawn point.
 * The boundary check is disabled while the dice is rolling.
 */
public class DiceBoundary : MonoBehaviour
{
    private Rigidbody rb;          // Rigidbody of the dice
    private Transform spawnPoint;  // Position where the dice should return
    private Transform playerDice;  // Area that defines the movement limits

    private DiceRoller roller;     // Reference to the dice roller component

    /*
     * Initializes the boundary with a spawn point and an area reference.
     */
    public void Init(Transform spawn, Transform area)
    {
        spawnPoint = spawn;
        playerDice = area;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        roller = GetComponent<DiceRoller>();
    }

    private void Update()
    {
        // Do not apply boundaries while the dice is rolling
        if (roller != null && roller.IsRolling())
            return;

        if (spawnPoint == null || playerDice == null)
            return;

        Vector3 center = playerDice.position;
        Vector3 size = playerDice.localScale;

        float halfX = size.x * 0.5f;
        float halfZ = size.z * 0.5f;

        Vector3 p = transform.position;

        // Check if the dice is outside the allowed region
        bool outX = p.x < center.x - halfX || p.x > center.x + halfX;
        bool outZ = p.z < center.z - halfZ || p.z > center.z + halfZ;
        bool outY = p.y < center.y - (size.y * 0.5f);

        if (outX || outZ || outY)
            Respawn();
    }

    /*
     * Respawns the dice at the spawn point and resets its physics.
     */
    private void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }


}
