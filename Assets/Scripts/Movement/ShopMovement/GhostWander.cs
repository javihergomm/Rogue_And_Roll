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
    public Color specialColor = Color.yellow;
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

    void OnEnable() => allGhosts.Add(this);
    void OnDisable() => allGhosts.Remove(this);

    void Start()
    {
        if (center != null)
        {
            Vector2 circle = Random.insideUnitCircle * maxDistance;
            transform.position = center.position + new Vector3(circle.x, 0f, circle.y);
        }

        baseY = transform.position.y;
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        SetVisible(true);

        if (isSpecial)
            SetColor(specialColor);

        noiseOffsetX = Random.Range(0f, 999f);
        noiseOffsetZ = Random.Range(0f, 999f);
    }

    void Update()
    {
        HandleMovement();
        HandleClick();
    }

    // -------------------------
    // MOVEMENT
    // -------------------------
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

        float nx = Mathf.PerlinNoise(noiseOffsetX, t) * 2f - 1f;
        float nz = Mathf.PerlinNoise(noiseOffsetZ, t) * 2f - 1f;
        Vector3 dir = new(nx, 0f, nz);

        Vector3 toCenter = center.position - transform.position;
        float dist = toCenter.magnitude;

        if (dist > maxDistance * 0.7f)
        {
            float lerp = Mathf.InverseLerp(maxDistance * 0.7f, maxDistance, dist);
            dir = Vector3.Lerp(dir, toCenter.normalized, lerp);
        }

        dir.Normalize();

        Vector3 newPos = transform.position + speed * Time.deltaTime * dir;
        newPos.y = baseY + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;

        transform.position = newPos;

        Vector3 flatDir = new(dir.x, 0f, dir.z);
        if (flatDir.sqrMagnitude > 0.001f)
            transform.forward = Vector3.Lerp(transform.forward, flatDir.normalized, Time.deltaTime * 2f);
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

    // -------------------------
    // CLICK + REWARD
    // -------------------------
    private void HandleClick()
    {
        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Camera cam = Camera.main;
        if (cam == null)
            return;

        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit, 100f))
            return;

        Transform hitTransform = hit.collider.transform;
        if (hitTransform != transform && !hitTransform.IsChildOf(transform))
            return;

        BaseItemSO reward = TryGiveReward();
        if (reward != null)
        {
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(reward, 1);
                SetColor(Color.white);
            }
        }
    }

    // -------------------------
    // HELPERS
    // -------------------------
    private void SetVisible(bool visible)
    {
        for (int i = 0; i < meshRenderers.Length; i++)
            meshRenderers[i].enabled = visible;
    }

    private void SetColor(Color c)
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            Material[] mats = meshRenderers[i].materials;
            for (int m = 0; m < mats.Length; m++)
            {
                if (mats[m].HasProperty("_Color"))
                    mats[m].color = c;
            }
        }
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
