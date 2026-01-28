using UnityEngine;
using System.Collections.Generic;

/*
 * DiceRollManager
 * ----------------
 * Central system that manages all physical dice in the world.
 *
 * Responsibilities:
 *  - Spawn dice prefabs in the scene based on active inventory slots
 *  - Remove dice when slots are cleared
 *  - Trigger physical dice rolls
 *  - Determine allowed faces for each dice based on all active effects
 *  - Apply synchronous and asynchronous dice effects
 *  - Store raw and final roll values for UI or debugging
 *
 * This class does NOT handle movement, UI, or inventory logic.
 * It only processes dice and produces the final roll result.
 */
public class DiceRollManager : MonoBehaviour
{
    public static DiceRollManager Instance { get; private set; }

    [Header("Dice Prefabs by Type")]
    [SerializeField] private List<GameObject> d4Prefabs;
    [SerializeField] private List<GameObject> d6Prefabs;
    [SerializeField] private List<GameObject> d8Prefabs;
    [SerializeField] private List<GameObject> d20Prefabs;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] activeDiceSpawnPoints;
    [SerializeField] private float spawnLift = 0.05f;

    [Header("Movement")]
    [SerializeField] private Movement playerMovement;

    private readonly Dictionary<ItemSlot, GameObject> worldDice = new();
    private readonly Dictionary<ItemSlot, (int baseRoll, int finalRoll)> rollHistory = new();

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
    // DICE SPAWNING
    // -------------------------------------------------------------------------

    public GameObject SpawnDiceInWorld(DiceSO dice, ItemSlot slot)
    {
        if (slot == null || dice == null || InventoryManager.Instance == null)
            return null;

        if (worldDice.ContainsKey(slot))
            return worldDice[slot];

        List<GameObject> prefabList = GetPrefabListForDice(dice.DiceType);
        if (prefabList == null || prefabList.Count == 0)
            return null;

        GameObject prefab = prefabList[0];
        if (prefab == null)
            return null;

        int index = InventoryManager.Instance.GetActiveDiceSlotIndex(slot);
        if (index < 0 || index >= activeDiceSpawnPoints.Length)
            return null;

        Transform spawnPoint = activeDiceSpawnPoints[index];
        if (spawnPoint == null)
            return null;

        GameObject instance = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);
        if (instance == null)
            return null;

        instance.transform.localScale = prefab.transform.localScale;

        DiceRoller roller = instance.GetComponent<DiceRoller>();
        if (roller != null)
            roller.AssignDice(dice, slot);

        AdjustSpawnHeight(instance);
        ResetPhysics(instance);

        worldDice[slot] = instance;
        return instance;
    }

    public void RemoveDiceFromWorld(ItemSlot slot)
    {
        if (!worldDice.ContainsKey(slot))
            return;

        Destroy(worldDice[slot]);
        worldDice.Remove(slot);
        rollHistory.Remove(slot);
    }

    public void RollDiceInSlot(ItemSlot slot)
    {
        if (!worldDice.ContainsKey(slot))
            return;

        DiceRoller roller = worldDice[slot].GetComponent<DiceRoller>();
        if (roller != null)
            roller.RollDice();
    }

    private void AdjustSpawnHeight(GameObject instance)
    {
        Collider col = instance.GetComponent<Collider>();
        if (col == null)
            return;

        float halfHeight = col.bounds.extents.y;
        Vector3 p = instance.transform.position;
        instance.transform.position = new Vector3(p.x, p.y + halfHeight + spawnLift, p.z);
    }

    private void ResetPhysics(GameObject instance)
    {
        Rigidbody rb = instance.GetComponent<Rigidbody>();
        if (rb == null)
            return;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.Sleep();
    }

    private List<GameObject> GetPrefabListForDice(DiceType type)
    {
        return type switch
        {
            DiceType.D4 => d4Prefabs,
            DiceType.D6 => d6Prefabs,
            DiceType.D8 => d8Prefabs,
            DiceType.D20 => d20Prefabs,
            _ => null
        };
    }

    // -------------------------------------------------------------------------
    // ALLOWED FACES
    // -------------------------------------------------------------------------

    public List<int> GetAllowedFacesForSlot(ItemSlot slot)
    {
        List<int> allowed = new();

        if (slot == null || InventoryManager.Instance == null || string.IsNullOrEmpty(slot.ItemName))
            return allowed;

        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.ItemName);
        if (item is not DiceSO dice)
            return allowed;

        int minAllowed = 1;
        int maxAllowed = dice.GetMaxFaceValue();

        DiceContext ctx = new()
        {
            turnNumber = StatManager.Instance.CurrentTurn,
            previousRoll = StatManager.Instance.PreviousRoll,
            slot = slot
        };

        ApplyEffectsToRange(dice.Effects, ref minAllowed, ref maxAllowed, ctx);
        ApplyPermanentEffectsToRange(ref minAllowed, ref maxAllowed, ctx);
        ApplyCharacterEffectsToRange(ref minAllowed, ref maxAllowed, ctx);
        ApplyConsumableEffectsToRange(ref minAllowed, ref maxAllowed, ctx);

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

    private void ApplyEffectsToRange(BaseEffect[] effects, ref int min, ref int max, DiceContext ctx)
    {
        if (effects == null)
            return;

        foreach (var eff in effects)
            if (eff is BaseDiceEffect diceEff)
                diceEff.ApplyToRange(ref min, ref max, ctx);
    }

    private void ApplyPermanentEffectsToRange(ref int min, ref int max, DiceContext ctx)
    {
        foreach (var s in InventoryManager.Instance.ItemSlots)
        {
            if (s.Quantity <= 0)
                continue;

            BaseItemSO it = InventoryManager.Instance.GetItemSO(s.ItemName);
            if (it is PermanentSO perm && perm.Effects != null)
                ApplyEffectsToRange(perm.Effects, ref min, ref max, ctx);
        }
    }

    private void ApplyCharacterEffectsToRange(ref int min, ref int max, DiceContext ctx)
    {
        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            eff.ApplyToRange(ref min, ref max, ctx);
    }

    private void ApplyConsumableEffectsToRange(ref int min, ref int max, DiceContext ctx)
    {
        foreach (var eff in StatManager.Instance.ActiveConsumableEffects)
            if (eff is BaseDiceEffect diceEff)
                diceEff.ApplyToRange(ref min, ref max, ctx);
    }

    // -------------------------------------------------------------------------
    // TARGET FACE SELECTION
    // -------------------------------------------------------------------------

    public int? GetTargetFaceForRoll(ItemSlot slot, int physicalRoll, DiceContext ctx)
    {
        if (slot == null || InventoryManager.Instance == null || string.IsNullOrEmpty(slot.ItemName))
            return null;

        List<int> allowed = GetAllowedFacesForSlot(slot);
        if (allowed == null || allowed.Count == 0)
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
        DiceContext ctx = new()
        {
            turnNumber = StatManager.Instance.CurrentTurn,
            previousRoll = StatManager.Instance.PreviousRoll,
            slot = slot
        };

        int finalRoll = ApplySynchronousEffects(slot, baseRoll, ctx);

        if (TryResolveAsyncEffects(slot, baseRoll, finalRoll, ctx))
            return;

        rollHistory[slot] = (baseRoll, finalRoll);
        FinalizeRoll(finalRoll);
    }

    private int ApplySynchronousEffects(ItemSlot slot, int roll, DiceContext ctx)
    {
        int result = roll;

        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.ItemName);
        if (item is DiceSO dice && dice.Effects != null)
            result = ApplySyncList(dice.Effects, result, ctx);

        foreach (var s in InventoryManager.Instance.ItemSlots)
        {
            if (s.Quantity <= 0)
                continue;

            BaseItemSO it = InventoryManager.Instance.GetItemSO(s.ItemName);
            if (it is PermanentSO perm && perm.Effects != null)
                result = ApplySyncList(perm.Effects, result, ctx);
        }

        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            if (!eff.RequiresAsyncResolution)
                result = eff.ModifyRoll(result, ctx);

        foreach (var eff in StatManager.Instance.ActiveConsumableEffects)
            if (eff is BaseDiceEffect diceEff && !diceEff.RequiresAsyncResolution)
                result = diceEff.ModifyRoll(result, ctx);

        return result;
    }

    private int ApplySyncList(BaseEffect[] effects, int roll, DiceContext ctx)
    {
        int result = roll;

        foreach (var eff in effects)
            if (eff is BaseDiceEffect diceEff && !diceEff.RequiresAsyncResolution)
                result = diceEff.ModifyRoll(result, ctx);

        return result;
    }

    private bool TryResolveAsyncEffects(ItemSlot slot, int baseRoll, int finalRoll, DiceContext ctx)
    {
        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.ItemName);
        if (item is DiceSO dice && dice.Effects != null)
            if (TryAsyncList(dice.Effects, slot, baseRoll, finalRoll, ctx))
                return true;

        foreach (var s in InventoryManager.Instance.ItemSlots)
        {
            if (s.Quantity <= 0)
                continue;

            BaseItemSO it = InventoryManager.Instance.GetItemSO(s.ItemName);
            if (it is PermanentSO perm && perm.Effects != null)
                if (TryAsyncList(perm.Effects, slot, baseRoll, finalRoll, ctx))
                    return true;
        }

        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            if (eff.RequiresAsyncResolution)
                return ResolveAsync(slot, baseRoll, finalRoll, ctx, eff);

        foreach (var eff in StatManager.Instance.ActiveConsumableEffects)
            if (eff is BaseDiceEffect diceEff && diceEff.RequiresAsyncResolution)
                return ResolveAsync(slot, baseRoll, finalRoll, ctx, diceEff);

        return false;
    }

    private bool TryAsyncList(BaseEffect[] effects, ItemSlot slot, int baseRoll, int finalRoll, DiceContext ctx)
    {
        foreach (var eff in effects)
        {
            if (eff is BaseDiceEffect diceEff && diceEff.RequiresAsyncResolution)
                return ResolveAsync(slot, baseRoll, finalRoll, ctx, diceEff);
        }

        return false;
    }

    private bool ResolveAsync(ItemSlot slot, int baseRoll, int finalRoll, DiceContext ctx, BaseDiceEffect eff)
    {
        eff.ModifyRollAsync(finalRoll, ctx, resolvedValue =>
        {
            rollHistory[slot] = (baseRoll, resolvedValue);
            FinalizeRoll(resolvedValue);
            InventoryManager.Instance.RefreshActiveDiceUI();
        });

        return true;
    }

    // -------------------------------------------------------------------------
    // PREVIEW
    // -------------------------------------------------------------------------

    public int GetFinalRollPreview(int rawRoll, DiceContext ctx, ItemSlot slot)
    {
        int result = rawRoll;

        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.ItemName);
        if (item is DiceSO dice && dice.Effects != null)
            result = ApplySyncList(dice.Effects, result, ctx);

        foreach (var s in InventoryManager.Instance.ItemSlots)
        {
            if (s.Quantity <= 0)
                continue;

            BaseItemSO it = InventoryManager.Instance.GetItemSO(s.ItemName);
            if (it is PermanentSO perm && perm.Effects != null)
                result = ApplySyncList(perm.Effects, result, ctx);
        }

        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            if (!eff.RequiresAsyncResolution)
                result = eff.ModifyRoll(result, ctx);

        foreach (var eff in StatManager.Instance.ActiveConsumableEffects)
            if (eff is BaseDiceEffect diceEff && !diceEff.RequiresAsyncResolution)
                result = diceEff.ModifyRoll(result, ctx);

        return result;
    }

    // -------------------------------------------------------------------------
    // PUBLIC ACCESS
    // -------------------------------------------------------------------------

    public (int baseRoll, int finalRoll)? GetRollInfo(ItemSlot slot)
    {
        if (rollHistory.TryGetValue(slot, out var info))
            return info;

        return null;
    }

    private void FinalizeRoll(int finalRoll)
    {
        Debug.Log("Final roll after all effects: " + finalRoll);

        // FIX: No direct setter access
        StatManager.Instance.OnDiceFinalResult(finalRoll);

        playerMovement.StartMoving();
    }

    // -------------------------------------------------------------------------
    // HIDE ROLL SUPPORT
    // -------------------------------------------------------------------------

    public bool IsRollHidden()
    {
        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            if (eff is HideRollEffect)
                return true;

        foreach (var eff in StatManager.Instance.ActiveConsumableEffects)
            if (eff is HideRollEffect)
                return true;

        return false;
    }
}
