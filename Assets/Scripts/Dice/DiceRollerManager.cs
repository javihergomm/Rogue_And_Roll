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

    private Dictionary<ItemSlot, GameObject> worldDice = new Dictionary<ItemSlot, GameObject>();
    private Dictionary<ItemSlot, (int baseRoll, int finalRoll)> rollHistory
        = new Dictionary<ItemSlot, (int baseRoll, int finalRoll)>();

    [SerializeField] private Movement playerMovement;

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

        List<GameObject> prefabList = GetPrefabListForDice(dice.diceType);
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

    private List<GameObject> GetPrefabListForDice(DiceType type)
    {
        switch (type)
        {
            case DiceType.D4: return d4Prefabs;
            case DiceType.D6: return d6Prefabs;
            case DiceType.D8: return d8Prefabs;
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

        if (slot == null || InventoryManager.Instance == null || string.IsNullOrEmpty(slot.itemName))
            return allowed;

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

        // Dice effects
        if (dice.effects != null)
        {
            foreach (var eff in dice.effects)
            {
                if (eff is BaseDiceEffect diceEff)
                    ApplyEffectToRange(diceEff, ref minAllowed, ref maxAllowed, ctx);
            }
        }

        // Permanent effects
        foreach (var s in InventoryManager.Instance.ItemSlots)
        {
            if (s.quantity > 0)
            {
                BaseItemSO it = InventoryManager.Instance.GetItemSO(s.itemName);
                if (it is PermanentSO perm && perm.effects != null)
                {
                    foreach (var eff in perm.effects)
                    {
                        if (eff is BaseDiceEffect diceEff)
                            ApplyEffectToRange(diceEff, ref minAllowed, ref maxAllowed, ctx);
                    }
                }
            }
        }

        // Character effects
        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
        {
            if (eff is BaseDiceEffect diceEff)
                ApplyEffectToRange(diceEff, ref minAllowed, ref maxAllowed, ctx);
        }

        // Consumable effects
        foreach (var effect in StatManager.Instance.ActiveConsumableEffects)
        {
            if (effect is BaseDiceEffect diceEff)
                ApplyEffectToRange(diceEff, ref minAllowed, ref maxAllowed, ctx);
        }

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
        effect.ApplyToRange(ref minAllowed, ref maxAllowed, ctx);
    }


    // -------------------------------------------------------------------------
    // TARGET FACE SELECTION
    // -------------------------------------------------------------------------

    public int? GetTargetFaceForRoll(ItemSlot slot, int physicalRoll, DiceContext ctx)
    {
        if (slot == null || InventoryManager.Instance == null || string.IsNullOrEmpty(slot.itemName))
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
    // FINAL ROLL PROCESSING (ASYNC READY)
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

        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.itemName);
        DiceSO dice = item as DiceSO;

        // -------------------------------
        // SYNCHRONOUS EFFECTS
        // -------------------------------

        // Dice effects
        if (dice != null && dice.effects != null)
        {
            foreach (var eff in dice.effects)
            {
                if (eff is BaseDiceEffect diceEff && !diceEff.RequiresAsyncResolution)
                    finalRoll = diceEff.ModifyRoll(finalRoll, ctx);
            }
        }

        // Permanent effects
        foreach (var s in InventoryManager.Instance.ItemSlots)
        {
            if (s.quantity > 0)
            {
                BaseItemSO it = InventoryManager.Instance.GetItemSO(s.itemName);
                if (it is PermanentSO perm && perm.effects != null)
                {
                    foreach (var eff in perm.effects)
                    {
                        if (eff is BaseDiceEffect diceEff && !diceEff.RequiresAsyncResolution)
                            finalRoll = diceEff.ModifyRoll(finalRoll, ctx);
                    }
                }
            }
        }

        // Character effects
        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
        {
            if (eff is BaseDiceEffect diceEff && !diceEff.RequiresAsyncResolution)
                finalRoll = diceEff.ModifyRoll(finalRoll, ctx);
        }

        // Consumable effects
        foreach (var effect in StatManager.Instance.ActiveConsumableEffects)
        {
            if (effect is BaseDiceEffect diceEff && !diceEff.RequiresAsyncResolution)
                finalRoll = diceEff.ModifyRoll(finalRoll, ctx);
        }

        // -------------------------------
        // ASYNCHRONOUS EFFECTS
        // -------------------------------

        // 1) Dice effects async
        if (dice != null && dice.effects != null)
        {
            foreach (var eff in dice.effects)
            {
                if (eff is BaseDiceEffect diceEff && diceEff.RequiresAsyncResolution)
                {
                    diceEff.ModifyRollAsync(finalRoll, ctx, (resolvedValue) =>
                    {
                        rollHistory[slot] = (baseRoll, resolvedValue);
                        FinalizeRoll(resolvedValue);
                        InventoryManager.Instance.RefreshActiveDiceUI();
                    });
                    return;
                }
            }
        }

        // 2) Permanent effects async
        foreach (var s in InventoryManager.Instance.ItemSlots)
        {
            if (s.quantity > 0)
            {
                BaseItemSO it = InventoryManager.Instance.GetItemSO(s.itemName);
                if (it is PermanentSO perm && perm.effects != null)
                {
                    foreach (var eff in perm.effects)
                    {
                        if (eff is BaseDiceEffect diceEff && diceEff.RequiresAsyncResolution)
                        {
                            diceEff.ModifyRollAsync(finalRoll, ctx, (resolvedValue) =>
                            {
                                rollHistory[slot] = (baseRoll, resolvedValue);
                                FinalizeRoll(resolvedValue);
                                InventoryManager.Instance.RefreshActiveDiceUI();
                            });
                            return;
                        }
                    }
                }
            }
        }

        // 3) Character effects async
        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
        {
            if (eff is BaseDiceEffect diceEff && diceEff.RequiresAsyncResolution)
            {
                diceEff.ModifyRollAsync(finalRoll, ctx, (resolvedValue) =>
                {
                    rollHistory[slot] = (baseRoll, resolvedValue);
                    FinalizeRoll(resolvedValue);
                    InventoryManager.Instance.RefreshActiveDiceUI();
                });
                return;
            }
        }

        // 4) Consumable effects async
        foreach (var effect in StatManager.Instance.ActiveConsumableEffects)
        {
            if (effect is BaseDiceEffect diceEff && diceEff.RequiresAsyncResolution)
            {
                diceEff.ModifyRollAsync(finalRoll, ctx, (resolvedValue) =>
                {
                    rollHistory[slot] = (baseRoll, resolvedValue);
                    FinalizeRoll(resolvedValue);
                    InventoryManager.Instance.RefreshActiveDiceUI();
                });
                return;
            }
        }

        // No async effects -> finalize immediately
        rollHistory[slot] = (baseRoll, finalRoll);
        FinalizeRoll(finalRoll);
    }

    // -------------------------------------------------------------------------
    // PREVIEW
    // -------------------------------------------------------------------------

    public int GetFinalRollPreview(int rawRoll, DiceContext ctx, ItemSlot slot)
    {
        int result = rawRoll;

        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.itemName);
        if (item is DiceSO dice && dice.effects != null)
        {
            foreach (var eff in dice.effects)
            {
                if (eff is BaseDiceEffect diceEff && !diceEff.RequiresAsyncResolution)
                    result = diceEff.ModifyRoll(result, ctx);
            }
        }

        foreach (var s in InventoryManager.Instance.ItemSlots)
        {
            if (s.quantity > 0)
            {
                BaseItemSO it = InventoryManager.Instance.GetItemSO(s.itemName);
                if (it is PermanentSO perm && perm.effects != null)
                {
                    foreach (var eff in perm.effects)
                    {
                        if (eff is BaseDiceEffect diceEff && !diceEff.RequiresAsyncResolution)
                            result = diceEff.ModifyRoll(result, ctx);
                    }
                }
            }
        }

        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
        {
            if (eff is BaseDiceEffect diceEff && !diceEff.RequiresAsyncResolution)
                result = diceEff.ModifyRoll(result, ctx);
        }

        foreach (var effect in StatManager.Instance.ActiveConsumableEffects)
        {
            if (effect is BaseDiceEffect diceEff && !diceEff.RequiresAsyncResolution)
                result = diceEff.ModifyRoll(result, ctx);
        }

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

        StatManager.Instance.PreviousRoll = finalRoll;
        StatManager.Instance.OnDiceFinalResult(finalRoll);
        playerMovement.StartMoving();


    }
    // -------------------------------------------------------------------------
    // HIDE ROLL SUPPORT
    // -------------------------------------------------------------------------

    public bool IsRollHidden()
    {
        // Check active dice effects
        foreach (var eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            if (eff is HideRollEffect)
                return true;

        // Check active consumable effects
        foreach (var eff in StatManager.Instance.ActiveConsumableEffects)
            if (eff is HideRollEffect)
                return true;

        return false;
    }


}