using UnityEngine;
using System.Collections.Generic;

public class GhostWander : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float noiseScale = 0.5f;     // amplitud del movimiento
    public float noiseSpeed = 0.5f;     // velocidad del movimiento

    [Header("Movement Area")]
    public Transform center;
    public float maxDistance = 3f;

    [Header("Vertical Float")]
    public float floatAmplitude = 0.5f;
    public float floatSpeed = 1f;

    [Header("Anti-Group Settings")]
    public float groupCheckDistance = 1.2f;
    public int maxAllowedNearby = 2;
    public float invisibleMoveTime = 1.5f;

    private bool isInvisible = false;
    private float invisibleTimer = 0f;

    private MeshRenderer[] meshRenderers;
    private float baseY;

    private static List<GhostWander> allGhosts = new();

    private float noiseOffsetX;
    private float noiseOffsetZ;

    void OnEnable() => allGhosts.Add(this);
    void OnDisable() => allGhosts.Remove(this);

    void Start()
    {
        // Aparición inicial dentro del área
        Vector2 circle = Random.insideUnitCircle * maxDistance;
        Vector3 startPos = center.position + new Vector3(circle.x, 0f, circle.y);
        transform.position = startPos;

        baseY = transform.position.y;

        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        SetVisible(true);

        // Offsets aleatorios para que cada fantasma tenga un movimiento único
        noiseOffsetX = Random.Range(0f, 999f);
        noiseOffsetZ = Random.Range(0f, 999f);
    }

    void Update()
    {
        if (isInvisible)
        {
            invisibleTimer -= Time.deltaTime;

            if (invisibleTimer <= 0f)
            {
                SetVisible(true);
                isInvisible = false;
            }

            MoveGhost();
            return;
        }

        // Anti-grupo: si hay 3 o más cerca, desaparecer
        int nearby = 0;
        foreach (var other in allGhosts)
        {
            if (other == this) continue;

            if (Vector3.Distance(transform.position, other.transform.position) < groupCheckDistance)
                nearby++;
        }

        if (nearby >= maxAllowedNearby)
        {
            StartDisappear();
            return;
        }

        MoveGhost();
    }

    private void MoveGhost()
    {
        float t = Time.time * noiseSpeed;

        // Movimiento horizontal suave con Perlin Noise
        float nx = Mathf.PerlinNoise(noiseOffsetX, t) * 2f - 1f;
        float nz = Mathf.PerlinNoise(noiseOffsetZ, t) * 2f - 1f;

        Vector3 dir = new Vector3(nx, 0f, nz).normalized;

        // Retorno suave al centro
        Vector3 toCenter = center.position - transform.position;
        float dist = toCenter.magnitude;

        if (dist > maxDistance * 0.7f)
        {
            float lerp = Mathf.InverseLerp(maxDistance * 0.7f, maxDistance, dist);
            dir = Vector3.Lerp(dir, toCenter.normalized, lerp);
        }

        // Movimiento horizontal
        Vector3 newPos = transform.position + speed * Time.deltaTime * dir;

        // Movimiento vertical suave
        newPos.y = baseY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        transform.position = newPos;

        // Rotación suave hacia la dirección de movimiento
        Vector3 flatDir = new(dir.x, 0f, dir.z);
        if (flatDir != Vector3.zero)
            transform.forward = Vector3.Lerp(transform.forward, flatDir.normalized, Time.deltaTime * 2f);
    }

    private void StartDisappear()
    {
        SetVisible(false);
        isInvisible = true;
        invisibleTimer = invisibleMoveTime;

        // Reposicionarlo dentro del área
        Vector2 circle = Random.insideUnitCircle * maxDistance;
        Vector3 newPos = center.position + new Vector3(circle.x, 0f, circle.y);
        newPos.y = baseY;
        transform.position = newPos;
    }

    private void SetVisible(bool visible)
    {
        if (meshRenderers == null) return;

        foreach (var r in meshRenderers)
            r.enabled = visible;
    }
}
