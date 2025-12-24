using UnityEngine;
using System.Collections.Generic;

/*
 * DiceRollManager
 * ----------------
 * Manages all dice that exist in the world.
 * Each active dice slot has exactly one world dice instance.
 * Dice appear when a slot becomes active and disappear when it stops being active.
 * Dice only roll when clicked by the player.
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
    [SerializeField] private Transform[] activeDiceSpawnPoints;
    [SerializeField] private float spawnLift = 0.05f;

    // One world dice per active slot
    private Dictionary<ItemSlot, GameObject> worldDice = new Dictionary<ItemSlot, GameObject>();

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
     * SpawnDiceInWorld
     * ----------------
     * Creates a dice instance for the given slot.
     * Does not roll automatically.
     */
    public GameObject SpawnDiceInWorld(DiceSO dice, ItemSlot slot)
    {
        if (worldDice.ContainsKey(slot))
            return worldDice[slot];

        List<GameObject> prefabList = GetPrefabListForDice(dice.diceType);
        if (prefabList == null || prefabList.Count == 0)
            return null;

        GameObject prefab = prefabList[0];

        int index = InventoryManager.Instance.GetActiveDiceSlotIndex(slot);
        if (index < 0 || index >= activeDiceSpawnPoints.Length)
            return null;

        Transform spawnPoint = activeDiceSpawnPoints[index];

        GameObject instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        instance.transform.localScale = prefab.transform.localScale;

        Collider col = instance.GetComponent<Collider>();
        if (col != null)
        {
            float halfHeight = col.bounds.extents.y;
            Vector3 p = instance.transform.position;
            instance.transform.position = new Vector3(p.x, p.y + halfHeight + spawnLift, p.z);
        }

        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.Sleep();
        }

        worldDice[slot] = instance;
        return instance;
    }

    /*
     * RemoveDiceFromWorld
     * -------------------
     * Deletes the dice instance associated with a slot.
     */
    public void RemoveDiceFromWorld(ItemSlot slot)
    {
        if (!worldDice.ContainsKey(slot))
            return;

        Destroy(worldDice[slot]);
        worldDice.Remove(slot);
    }

    /*
     * RollDiceInSlot
     * --------------
     * Rolls the dice associated with the given slot.
     */
    public void RollDiceInSlot(ItemSlot slot)
    {
        if (!worldDice.ContainsKey(slot))
            return;

        GameObject die = worldDice[slot];
        DiceRoller roller = die.GetComponent<DiceRoller>();
        if (roller != null)
            roller.RollDice();
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
        }
        return null;
    }
}
