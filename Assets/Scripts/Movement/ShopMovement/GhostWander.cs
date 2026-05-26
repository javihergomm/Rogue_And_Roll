using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GhostWander : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 2f;
    public float noiseScale = 0.5f;
    public float noiseSpeed = 0.5f;

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

    [Header("Special Ghost")]
    public bool isSpecial = false;
    public BaseItemSO rewardA;
    public BaseItemSO rewardB;
    public float rewardChance = 1f;

    private bool rewardGiven = false;
    private bool isInvisible = false;
    private float invisibleTimer = 0f;

    private MeshRenderer[] meshRenderers;
    private float baseY;

    private static readonly List<GhostWander> allGhosts = new();

    private float noiseOffsetX;
    private float noiseOffsetZ;

    private Vector3 smoothDir;

    void OnEnable() => allGhosts.Add(this);
    void OnDisable() => allGhosts.Remove(this);

    void Start()
    {
        // Auto-asignar GhostCreator si no está asignado
        if (center == null)
        {
            GameObject creator = GameObject.Find("GhostCreator");
            if (creator != null)
                center = creator.transform;
            else
                Debug.LogError("GhostWander: No se encontró GhostCreator en la escena.");
        }

        // Ajustar radio según cámara
        Camera cam = Camera.main;
        if (cam != null && center != null)
        {
            float dist = Vector3.Distance(cam.transform.position, center.position);
            float visibleRadius = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * dist;

            maxDistance = visibleRadius * 0.4f; // 40% del área visible
        }

        // Posición inicial dentro del círculo
        if (center != null)
        {
            Vector2 circle = Random.insideUnitCircle * maxDistance;
            transform.position = center.position + new Vector3(circle.x, 0f, circle.y);
        }

        baseY = transform.position.y;
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        SetVisible(true);

        noiseOffsetX = Random.Range(0f, 999f);
        noiseOffsetZ = Random.Range(0f, 999f);

        smoothDir = transform.forward;
    }


    void Update()
    {
        HandleMovement();
        HandleClick();
    }

    private void HandleMovement()
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

        int nearby = 0;
        for (int i = 0; i < allGhosts.Count; i++)
        {
            GhostWander other = allGhosts[i];
            if (other != this &&
                Vector3.Distance(transform.position, other.transform.position) < groupCheckDistance)
            {
                nearby++;
            }
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

        // 1. Perlin Noise -> dirección deseada (solo para ROTAR)
        float nx = Mathf.PerlinNoise(noiseOffsetX, t) * 2f - 1f;
        float nz = Mathf.PerlinNoise(noiseOffsetZ, t) * 2f - 1f;
        Vector3 desiredDir = new Vector3(nx, 0f, nz).normalized;

        // 2. Corrección hacia el centro si se aleja demasiado
        Vector3 toCenter = center.position - transform.position;
        float dist = toCenter.magnitude;

        if (dist > maxDistance * 0.7f)
        {
            float lerp = Mathf.InverseLerp(maxDistance * 0.7f, maxDistance, dist);
            desiredDir = Vector3.Lerp(desiredDir, toCenter.normalized, lerp).normalized;
        }

        // 3. Evitar giros hacia atrás respecto al forward visual
        if (Vector3.Dot(transform.forward, desiredDir) < 0f)
        {
            desiredDir = Vector3.ProjectOnPlane(desiredDir, Vector3.up);
            desiredDir = Vector3.Lerp(transform.forward, desiredDir, 0.5f).normalized;
        }

        // 4. Suavizado de dirección
        smoothDir = Vector3.Lerp(smoothDir, desiredDir, Time.deltaTime * 2f);

        // 5. Reforzar que smoothDir nunca quede detrás
        if (Vector3.Dot(transform.forward, smoothDir) < 0f)
        {
            smoothDir = Vector3.Lerp(transform.forward, smoothDir, 0.3f).normalized;
        }

        // 6. Rotación suave hacia smoothDir
        float maxTurnSpeed = 120f;
        Quaternion targetRot = Quaternion.LookRotation(smoothDir, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            maxTurnSpeed * Time.deltaTime
        );

        // 7. Movimiento SIEMPRE hacia delante visual 
        Vector3 forwardDir = -transform.forward; 
        Vector3 newPos = transform.position + speed * Time.deltaTime * forwardDir;

        // 8. Flotación vertical
        newPos.y = baseY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        transform.position = newPos;
    }


    private void StartDisappear()
    {
        SetVisible(false);
        isInvisible = true;
        invisibleTimer = invisibleMoveTime;

        Vector2 circle = Random.insideUnitCircle * maxDistance;
        Vector3 newPos = center.position + new Vector3(circle.x, 0f, circle.y);
        newPos.y = baseY;
        transform.position = newPos;
    }

    private void HandleClick()
    {
        if (Mouse.current == null ||
            !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (!Physics.Raycast(ray, out RaycastHit hit, 100f))
            return;

        if (hit.collider.transform != transform &&
            !hit.collider.transform.IsChildOf(transform))
            return;

        BaseItemSO reward = TryGiveReward();
        if (reward != null && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(reward, 1);
        }
    }

    private void SetVisible(bool visible)
    {
        foreach (var r in meshRenderers)
            r.enabled = visible;
    }

    public BaseItemSO TryGiveReward()
    {
        if (!isSpecial || rewardGiven)
            return null;

        if (Random.value > rewardChance)
            return null;

        rewardGiven = true;

        if (rewardA != null && rewardB == null) return rewardA;
        if (rewardB != null && rewardA == null) return rewardB;
        if (rewardA != null && rewardB != null)
            return Random.value < 0.5f ? rewardA : rewardB;

        return null;
    }
}
