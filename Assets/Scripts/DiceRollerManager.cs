using UnityEngine;
using System.Collections.Generic;

/*
 * DiceRollManager
 * ----------------
 * Responsible for spawning and rolling dice prefabs.
 * - Supports multiple prefab variants per dice type (e.g. different D6 or D4 designs).
 * - Ensures only one active dice exists at a time.
 * - Resets scale to prefab defaults when spawning.
 * - Tracks which DiceSO is active for UI feedback.
 * - Keeps the active dice slot highlighted.
 */
public class DiceRollManager : MonoBehaviour
{
    public static DiceRollManager Instance { get; private set; }

    [Header("Dice Prefabs by Type")]
    [SerializeField] private List<GameObject> d4Prefabs;
    [SerializeField] private List<GameObject> d6Prefabs;
    [SerializeField] private List<GameObject> d8Prefabs;
    [SerializeField] private List<GameObject> d10Prefabs;
    [SerializeField] private List<GameObject> d12Prefabs;
    [SerializeField] private List<GameObject> d20Prefabs;

    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float spawnLift = 0.05f; // lift dice slightly above surfaces

    // Reference to the currently active dice prefab
    private GameObject activeDice;

    // Reference to the currently active dice data
    public DiceSO ActiveDiceSO { get; private set; }

    // Reference to the currently active slot in UI
    private ItemSlot activeDiceSlot;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /*
     * SpawnDice
     * ---------
     * Instantiates the correct prefab for the given DiceSO.
     * Does NOT roll automatically.
     */
    public void SpawnDice(DiceSO dice, int variantIndex = 0, ItemSlot slot = null)
    {
        if (activeDice != null)
        {
            Debug.Log("[DiceRollManager] Destroying previous dice: " + activeDice.name);
            Destroy(activeDice);
            activeDice = null;
        }

        List<GameObject> prefabList = GetPrefabListForDice(dice.diceType);
        if (prefabList == null || prefabList.Count == 0)
        {
            Debug.LogWarning("[DiceRollManager] No prefabs found for dice type: " + dice.diceType);
            return;
        }

        if (variantIndex < 0 || variantIndex >= prefabList.Count) variantIndex = 0;
        GameObject prefab = prefabList[variantIndex];

        // Instantiate new dice
        activeDice = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        activeDice.transform.localScale = prefab.transform.localScale;

        Debug.Log("[DiceRollManager] Spawned dice prefab: " + prefab.name + " at " + spawnPoint.position);

        // Lift dice above board to avoid collider overlap
        Collider col = activeDice.GetComponent<Collider>();
        if (col != null)
        {
            float halfHeight = col.bounds.extents.y;
            Vector3 p = activeDice.transform.position;
            activeDice.transform.position = new Vector3(p.x, p.y + halfHeight + spawnLift, p.z);

            Debug.Log("[DiceRollManager] Adjusted spawn height by " + (halfHeight + spawnLift) +
                      " | Final position: " + activeDice.transform.position);
        }

        // Reset Rigidbody state
        Rigidbody rb = activeDice.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log("[DiceRollManager] Rigidbody settings -> isKinematic: " + rb.isKinematic +
                      ", useGravity: " + rb.useGravity +
                      ", velocity: " + rb.linearVelocity +
                      ", angularVelocity: " + rb.angularVelocity);

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();

            Debug.Log("[DiceRollManager] Rigidbody reset complete. Dice should be idle.");
        }

        // Track active dice data
        ActiveDiceSO = dice;

        // Highlight slot in UI
        if (slot != null)
        {
            HighlightActiveDiceSlot(slot);
        }
    }

    /*
     * RollActiveDice
     * --------------
     * Applies roll logic to the currently active dice.
     */
    public void RollActiveDice()
    {
        if (activeDice == null)
        {
            Debug.LogWarning("[DiceRollManager] No active dice to roll.");
            return;
        }

        DiceRoller roller = activeDice.GetComponent<DiceRoller>();
        if (roller != null)
        {
            Debug.Log("[DiceRollManager] Rolling active dice: " + activeDice.name);
            roller.RollDice();
        }
        else
        {
            Debug.LogWarning("[DiceRollManager] DiceRoller component missing on active dice.");
        }
    }

    /*
     * HighlightActiveDiceSlot
     * -----------------------
     * Keeps the active dice slot marked in the UI.
     */
    private void HighlightActiveDiceSlot(ItemSlot slot)
    {
        if (activeDiceSlot != null && activeDiceSlot != slot)
        {
            activeDiceSlot.DeselectSlot();
        }

        activeDiceSlot = slot;
        activeDiceSlot.SelectSlot();

        Debug.Log("[DiceRollManager] Active dice slot highlighted: " + slot.itemName);
    }

    private List<GameObject> GetPrefabListForDice(DiceType type)
    {
        switch (type)
        {
            case DiceType.D4: return d4Prefabs;
            case DiceType.D6: return d6Prefabs;
            case DiceType.D8: return d8Prefabs;
            case DiceType.D10: return d10Prefabs;
            case DiceType.D12: return d12Prefabs;
            case DiceType.D20: return d20Prefabs;
            default: return null;
        }
    }

    public void ClearActiveDice()
    {
        if (activeDice != null)
        {
            Debug.Log("[DiceRollManager] Clearing active dice: " + activeDice.name);
            Destroy(activeDice);
            activeDice = null;
            ActiveDiceSO = null;

            if (activeDiceSlot != null)
            {
                activeDiceSlot.DeselectSlot();
                activeDiceSlot = null;
            }
        }
    }
}
