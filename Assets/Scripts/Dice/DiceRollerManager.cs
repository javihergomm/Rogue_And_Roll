using UnityEngine;
using System.Collections.Generic;

/*
 * DiceRollManager
 * ----------------
 * Handles the physical dice that exist in the world.
 * Each active dice slot has one physical dice instance.
 * Responsible for spawning, removing, and rolling dice.
 * Determines the allowed faces for each dice based on active effects.
 * Processes the final roll result and forwards it to the StatManager.
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

    // -------------------------------------------------------------------------
    // DEBUG
    // -------------------------------------------------------------------------
    private void DebugActiveEffects()
    {
        Debug.Log("========== DEBUG ROLL EFFECTS ==========");

        // Consumables
        if (StatManager.Instance.ActiveConsumableEffects.Count == 0)
            Debug.Log("Active consumables: none");
        else
        {
            Debug.Log("Active consumables:");
            foreach (var eff in StatManager.Instance.ActiveConsumableEffects)
                Debug.Log(" - " + eff.GetType().Name);
        }

        // Permanents
        Debug.Log("Active permanents:");
        foreach (var slot in InventoryManager.Instance.ItemSlots)
        {
            if (slot.quantity > 0)
            {
                BaseItemSO it = InventoryManager.Instance.GetItemSO(slot.itemName);
                if (it is PermanentSO perm && perm.diceEffect != null)
                    Debug.Log(" - " + perm.itemName + " (" + perm.diceEffect.GetType().Name + ")");
            }
        }

        // Character effects
        Debug.Log("Character dice effects:");
        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            Debug.Log(" - " + eff.GetType().Name);

        // Active dice effect
        ItemSlot activeSlot = InventoryManager.Instance.ActiveDiceSlot;
        if (activeSlot != null && activeSlot.quantity > 0)
        {
            BaseItemSO item = InventoryManager.Instance.GetItemSO(activeSlot.itemName);
            if (item is DiceSO dice && dice.diceEffect != null)
                Debug.Log("Active dice effect: " + dice.diceEffect.GetType().Name);
            else
                Debug.Log("Active dice effect: none");
        }
        else
        {
            Debug.Log("No active dice");
        }

        Debug.Log("========================================");
    }

    // -------------------------------------------------------------------------
    // DICE SPAWNING
    // -------------------------------------------------------------------------

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

        DiceRoller roller = instance.GetComponent<DiceRoller>();
        if (roller != null)
            roller.AssignDice(dice, slot);

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

    public void RemoveDiceFromWorld(ItemSlot slot)
    {
        if (!worldDice.ContainsKey(slot))
            return;

        Destroy(worldDice[slot]);
        worldDice.Remove(slot);
    }

    public void RollDiceInSlot(ItemSlot slot)
    {
        if (!worldDice.ContainsKey(slot))
            return;

        DiceRoller roller = worldDice[slot].GetComponent<DiceRoller>();
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

    // -------------------------------------------------------------------------
    // ALLOWED FACES
    // -------------------------------------------------------------------------

    public List<int> GetAllowedFacesForSlot(ItemSlot slot)
    {
        List<int> allowed = new List<int>();

        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.itemName);
        if (!(item is DiceSO dice))
            return allowed;

        int minAllowed = 1;
        int maxAllowed = dice.GetMaxFaceValue();

        DiceContext ctx = new DiceContext
        {
            turnNumber = StatManager.Instance.CurrentTurn,
            previousRoll = StatManager.Instance.PreviousRoll,
            slot = slot
        };

        // Dice effect
        if (dice.diceEffect != null)
            ApplyEffectToRange(dice.diceEffect, ref minAllowed, ref maxAllowed, ctx);

        // Permanent effects
        foreach (var s in InventoryManager.Instance.ItemSlots)
        {
            if (s.quantity > 0)
            {
                BaseItemSO it = InventoryManager.Instance.GetItemSO(s.itemName);
                if (it is PermanentSO perm && perm.diceEffect != null)
                    ApplyEffectToRange(perm.diceEffect, ref minAllowed, ref maxAllowed, ctx);
            }
        }

        // Character dice effects
        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            ApplyEffectToRange(eff, ref minAllowed, ref maxAllowed, ctx);

        // Consumable effects
        foreach (var effect in StatManager.Instance.ActiveConsumableEffects)
            ApplyEffectToRange(effect, ref minAllowed, ref maxAllowed, ctx);

        // Build allowed list
        for (int face = 1; face <= dice.GetMaxFaceValue(); face++)
        {
            if (face >= minAllowed && face <= maxAllowed)
                allowed.Add(face);
        }

        return allowed;
    }

    public bool IsFaceAllowed(ItemSlot slot, int face)
    {
        return GetAllowedFacesForSlot(slot).Contains(face);
    }

    private void ApplyEffectToRange(BaseDiceEffect effect, ref int minAllowed, ref int maxAllowed, DiceContext ctx)
    {
        if (effect is MinValueDiceEffect minEff)
            minAllowed = Mathf.Max(minAllowed, minEff.MinValue);

        else if (effect is MaxValueDiceEffect maxEff)
            maxAllowed = Mathf.Min(maxAllowed, maxEff.MaxValue);

        else if (effect is MultiplierDiceEffect)
        {
            // Multipliers do not restrict allowed faces.
        }
    }

    // -------------------------------------------------------------------------
    // INTELLIGENT TARGET FACE SELECTION
    // -------------------------------------------------------------------------

    public int? GetTargetFaceForRoll(ItemSlot slot, int physicalRoll, DiceContext ctx)
    {
        List<int> allowed = GetAllowedFacesForSlot(slot);

        if (allowed.Count == 0)
            return null;

        int preview = GetFinalRollPreview(physicalRoll, ctx, slot);

        if (allowed.Contains(preview))
            return preview;

        int closest = allowed[0];
        int bestDist = Mathf.Abs(preview - closest);

        for (int i = 1; i < allowed.Count; i++)
        {
            int dist = Mathf.Abs(preview - allowed[i]);
            if (dist < bestDist)
            {
                bestDist = dist;
                closest = allowed[i];
            }
        }

        return closest;
    }

    // -------------------------------------------------------------------------
    // FINAL ROLL PROCESSING
    // -------------------------------------------------------------------------

    public void OnDiceResult(ItemSlot slot, int baseRoll)
    {
        DiceContext ctx = new DiceContext
        {
            turnNumber = StatManager.Instance.CurrentTurn,
            previousRoll = StatManager.Instance.PreviousRoll,
            slot = slot
        };

        int finalRoll = baseRoll;

        Debug.Log("========== ROLL PROCESSING ==========");
        Debug.Log("Base roll (physics): " + baseRoll);

        DebugActiveEffects();

        // Dice effect
        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.itemName);
        if (item is DiceSO dice && dice.diceEffect != null)
            finalRoll = dice.diceEffect.ModifyRoll(finalRoll, ctx);

        // Permanent effects
        foreach (var s in InventoryManager.Instance.ItemSlots)
        {
            if (s.quantity > 0)
            {
                BaseItemSO it = InventoryManager.Instance.GetItemSO(s.itemName);
                if (it is PermanentSO perm && perm.diceEffect != null)
                    finalRoll = perm.diceEffect.ModifyRoll(finalRoll, ctx);
            }
        }

        // Character dice effects
        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            finalRoll = eff.ModifyRoll(finalRoll, ctx);

        // Consumable effects
        foreach (var effect in StatManager.Instance.ActiveConsumableEffects)
            finalRoll = effect.ModifyRoll(finalRoll, ctx);

        Debug.Log("Final roll after all effects: " + finalRoll);
        Debug.Log("=====================================");

        StatManager.Instance.PreviousRoll = finalRoll;
        StatManager.Instance.OnDiceFinalResult(finalRoll);
    }

    // -------------------------------------------------------------------------
    // PREVIEW
    // -------------------------------------------------------------------------

    public int GetFinalRollPreview(int rawRoll, DiceContext ctx, ItemSlot slot)
    {
        int result = rawRoll;

        // Dice effect
        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.itemName);
        if (item is DiceSO dice && dice.diceEffect != null)
            result = dice.diceEffect.ModifyRoll(result, ctx);

        // Permanent effects
        foreach (var s in InventoryManager.Instance.ItemSlots)
        {
            if (s.quantity > 0)
            {
                BaseItemSO it = InventoryManager.Instance.GetItemSO(s.itemName);
                if (it is PermanentSO perm && perm.diceEffect != null)
                    result = perm.diceEffect.ModifyRoll(result, ctx);
            }
        }

        // Character dice effects
        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            result = eff.ModifyRoll(result, ctx);

        // Consumable effects
        foreach (var effect in StatManager.Instance.ActiveConsumableEffects)
            result = effect.ModifyRoll(result, ctx);

        return result;
    }
}
