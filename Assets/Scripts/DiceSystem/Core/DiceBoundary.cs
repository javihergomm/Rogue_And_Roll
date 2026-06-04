using UnityEngine;

public class DiceBoundary : MonoBehaviour
{
    private Rigidbody rb;

    private Transform spawnPoint;
    private Transform playerDice;

    private DiceRoller roller; // referencia al roller

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
        // Si está rodando, NO aplicar límites
        if (roller != null && roller.IsRolling())
            return;

        if (spawnPoint == null || playerDice == null)
            return;

        Vector3 center = playerDice.position;
        Vector3 size = playerDice.localScale;

        float halfX = size.x * 0.5f;
        float halfZ = size.z * 0.5f;

        Vector3 p = transform.position;

        bool outX = p.x < center.x - halfX || p.x > center.x + halfX;
        bool outZ = p.z < center.z - halfZ || p.z > center.z + halfZ;
        bool outY = p.y < center.y - (size.y * 0.5f);

        if (outX || outZ || outY)
            Respawn();
    }

    private void Respawn()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();

        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }

    public void ForceRespawn()
    {
        Respawn();
    }
}
