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

    [Header("Board Area Limit")]
    public float boardRadius = 1f;     
    public float extraMargin = 1f;     

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
        // Auto-assign GhostCreator if not set
        if (center == null)
        {
            GameObject creator = GameObject.Find("GhostCreator");
            if (creator != null)
                center = creator.transform;
        }

        // Fixed board-based radius
        maxDistance = boardRadius + extraMargin;

        // Initial position inside the circle
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

        // 1. Perlin Noise -> desired direction (rotation only)
        float nx = Mathf.PerlinNoise(noiseOffsetX, t) * 2f - 1f;
        float nz = Mathf.PerlinNoise(noiseOffsetZ, t) * 2f - 1f;
        Vector3 desiredDir = new Vector3(nx, 0f, nz).normalized;

        // 2. Correction toward center if too far
        Vector3 toCenter = center.position - transform.position;
        float dist = toCenter.magnitude;

        if (dist > maxDistance * 0.7f)
        {
            float lerp = Mathf.InverseLerp(maxDistance * 0.7f, maxDistance, dist);
            desiredDir = Vector3.Lerp(desiredDir, toCenter.normalized, lerp).normalized;
        }

        // 3. Prevent backward turns
        if (Vector3.Dot(transform.forward, desiredDir) < 0f)
        {
            desiredDir = Vector3.ProjectOnPlane(desiredDir, Vector3.up);
            desiredDir = Vector3.Lerp(transform.forward, desiredDir, 0.5f).normalized;
        }

        // 4. Smooth direction
        smoothDir = Vector3.Lerp(smoothDir, desiredDir, Time.deltaTime * 2f);

        // 5. Ensure smoothDir never goes behind
        if (Vector3.Dot(transform.forward, smoothDir) < 0f)
        {
            smoothDir = Vector3.Lerp(transform.forward, smoothDir, 0.3f).normalized;
        }

        // 6. Smooth rotation
        float maxTurnSpeed = 120f;
        Quaternion targetRot = Quaternion.LookRotation(smoothDir, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            maxTurnSpeed * Time.deltaTime
        );

        // 7. Always move forward visually
        Vector3 forwardDir = -transform.forward;
        Vector3 newPos = transform.position + speed * Time.deltaTime * forwardDir;

        // 8. Vertical float
        newPos.y = baseY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        // 9. Clamp inside board radius
        Vector3 flatPos = new Vector3(newPos.x, center.position.y, newPos.z);
        Vector3 flatToCenter = flatPos - center.position;

        if (flatToCenter.magnitude > maxDistance)
        {
            flatToCenter = flatToCenter.normalized * maxDistance;
            newPos = center.position + flatToCenter;
            newPos.y = baseY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        }

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

        if (hit.collider.GetComponentInParent<GhostWander>() != this)
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
