using UnityEngine;
using System.Collections.Generic;

public class DiceRollManager : MonoBehaviour
{
    public static DiceRollManager Instance { get; private set; }

    [Header("Dice Prefabs")]
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

    private readonly Dictionary<ItemSlot, DiceCached> cachedDice = new();

    private readonly Dictionary<ItemSlot, (int baseRoll, int finalRoll)> rollHistory = new();

    private readonly HashSet<ItemSlot> rolledThisTurn = new();

    private struct DiceCached
    {
        public DiceRoller roller;
        public Rigidbody body;
        public Collider col;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (playerMovement != null)
            playerMovement.OnMovementFinished += OnPlayerFinishedMovement;
    }

    public void RegisterPlayerMovement(Movement movement)
    {
        playerMovement = movement;

        if (playerMovement != null)
            playerMovement.OnMovementFinished += OnPlayerFinishedMovement;
    }

    private void OnPlayerFinishedMovement()
    {
        ResetDiceTurnState();

        if (EnemyManager.Instance != null)
            EnemyManager.Instance.StartEnemyTurns();
    }

    // -------------------------------------------------------------------------
    // DICE SPAWNING
    // -------------------------------------------------------------------------

    public GameObject SpawnDiceInWorld(DiceSO dice, ItemSlot slot)
    {
        if (slot == null || dice == null || InventoryManager.Instance == null)
            return null;

        if (worldDice.TryGetValue(slot, out GameObject existing))
            return existing;

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
        instance.transform.localScale = prefab.transform.localScale;

        if (BoardHider.Instance != null)
            BoardHider.Instance.RegisterObject(instance);

        DiceCached cache = new()
        {
            roller = instance.GetComponent<DiceRoller>(),
            body = instance.GetComponent<Rigidbody>(),
            col = instance.GetComponent<Collider>()
        };

        cachedDice[slot] = cache;

        if (cache.roller != null)
            cache.roller.AssignDice(dice, slot);

        AdjustSpawnHeight(instance, cache.col);
        ResetPhysics(cache.body);

        worldDice[slot] = instance;
        return instance;
    }

    public void RemoveDiceFromWorld(ItemSlot slot)
    {
        if (!worldDice.ContainsKey(slot))
            return;

        GameObject obj = worldDice[slot];
        if (obj != null)
            Destroy(obj);

        worldDice.Remove(slot);
        cachedDice.Remove(slot);
        rollHistory.Remove(slot);
    }

    private void AdjustSpawnHeight(GameObject instance, Collider col)
    {
        if (col == null)
            return;

        float halfHeight = col.bounds.extents.y;
        Vector3 p = instance.transform.position;
        instance.transform.position = new Vector3(p.x, p.y + halfHeight + spawnLift, p.z);
    }

    private void ResetPhysics(Rigidbody rb)
    {
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
            _ => null,
        };
    }

    // -------------------------------------------------------------------------
    // ROLL ALL DICE
    // -------------------------------------------------------------------------

    public void RollAllActiveDice()
    {
        foreach (ItemSlot slot in InventoryManager.Instance.ActiveDice.GetNonEmptySlots())
        {
            if (!worldDice.ContainsKey(slot))
                continue;

            DiceCached cache = cachedDice[slot];

            if (cache.roller != null)
            {
                cache.roller.RollDice();
                cache.roller.StartRollRoutine();
            }
        }
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
        DiceSO dice = item as DiceSO;
        if (dice == null)
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
        ApplyCharacterEffectsToRange(ref minAllowed, ref maxAllowed, ctx);
        ApplyConsumableEffectsToRange(ref minAllowed, ref maxAllowed, ctx);

        for (int face = 1; face <= dice.GetMaxFaceValue(); face++)
            if (face >= minAllowed && face <= maxAllowed)
                allowed.Add(face);

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

        foreach (BaseEffect eff in effects)
        {
            BaseDiceEffect diceEff = eff as BaseDiceEffect;
            if (diceEff != null)
                diceEff.ApplyToRange(ref min, ref max, ctx);
        }
    }

    private void ApplyCharacterEffectsToRange(ref int min, ref int max, DiceContext ctx)
    {
        foreach (BaseDiceEffect eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            eff.ApplyToRange(ref min, ref max, ctx);
    }

    private void ApplyConsumableEffectsToRange(ref int min, ref int max, DiceContext ctx)
    {
        foreach (BaseEffect eff in StatManager.Instance.ActiveConsumableEffects)
        {
            BaseDiceEffect diceEff = eff as BaseDiceEffect;
            if (diceEff != null)
                diceEff.ApplyToRange(ref min, ref max, ctx);
        }
    }

    // -------------------------------------------------------------------------
    // ROLL PROCESSING
    // -------------------------------------------------------------------------

    public void OnDiceResult(ItemSlot slot, int baseRoll)
    {
        DiceContext diceContext = new()
        {
            turnNumber = StatManager.Instance.CurrentTurn,
            previousRoll = StatManager.Instance.PreviousRoll,
            slot = slot,
            IsFinal = true
        };
        DiceContext ctx = diceContext;

        int finalRoll = ApplySynchronousEffects(slot, baseRoll, ctx);

        if (TryResolveAsyncEffects(slot, baseRoll, finalRoll, ctx))
            return;

        rolledThisTurn.Add(slot);
        rollHistory[slot] = (baseRoll, finalRoll);
        FinalizeRoll(finalRoll);
    }

    private int ApplySynchronousEffects(ItemSlot slot, int roll, DiceContext ctx)
    {
        int result = roll;

        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.ItemName);
        DiceSO dice = item as DiceSO;
        if (dice != null && dice.Effects != null)
            result = ApplySyncList(dice.Effects, result, ctx);

        foreach (BaseDiceEffect eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            if (!eff.RequiresAsyncResolution)
                result = eff.ModifyRoll(result, ctx);

        foreach (BaseEffect eff in StatManager.Instance.ActiveConsumableEffects)
        {
            BaseDiceEffect diceEff = eff as BaseDiceEffect;
            if (diceEff != null && !diceEff.RequiresAsyncResolution)
                result = diceEff.ModifyRoll(result, ctx);
        }

        return result;
    }

    private int ApplySyncList(BaseEffect[] effects, int roll, DiceContext ctx)
    {
        int result = roll;

        foreach (BaseEffect eff in effects)
        {
            BaseDiceEffect diceEff = eff as BaseDiceEffect;
            if (diceEff != null && !diceEff.RequiresAsyncResolution)
                result = diceEff.ModifyRoll(result, ctx);
        }

        return result;
    }

    private bool TryResolveAsyncEffects(ItemSlot slot, int baseRoll, int finalRoll, DiceContext ctx)
    {
        BaseItemSO item = InventoryManager.Instance.GetItemSO(slot.ItemName);
        DiceSO dice = item as DiceSO;

        if (dice != null && dice.Effects != null)
            if (TryAsyncList(dice.Effects, slot, baseRoll, finalRoll, ctx))
                return true;

        foreach (BaseDiceEffect eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            if (eff.RequiresAsyncResolution)
                return ResolveAsync(slot, baseRoll, finalRoll, ctx, eff);

        foreach (BaseEffect eff in StatManager.Instance.ActiveConsumableEffects)
        {
            BaseDiceEffect diceEff = eff as BaseDiceEffect;
            if (diceEff != null && diceEff.RequiresAsyncResolution)
                return ResolveAsync(slot, baseRoll, finalRoll, ctx, diceEff);
        }

        return false;
    }

    private bool TryAsyncList(BaseEffect[] effects, ItemSlot slot, int baseRoll, int finalRoll, DiceContext ctx)
    {
        foreach (BaseEffect eff in effects)
        {
            BaseDiceEffect diceEff = eff as BaseDiceEffect;
            if (diceEff != null && diceEff.RequiresAsyncResolution)
                return ResolveAsync(slot, baseRoll, finalRoll, ctx, diceEff);
        }

        return false;
    }

    private bool ResolveAsync(ItemSlot slot, int baseRoll, int finalRoll, DiceContext ctx, BaseDiceEffect eff)
    {
        eff.ModifyRollAsync(finalRoll, ctx, delegate (int resolvedValue)
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
        DiceSO dice = item as DiceSO;

        if (dice != null && dice.Effects != null)
            result = ApplySyncList(dice.Effects, result, ctx);

        foreach (BaseDiceEffect eff in CharacterEffectManager.Instance.ActiveDiceEffects)
            if (!eff.RequiresAsyncResolution)
                result = eff.ModifyRoll(result, ctx);

        foreach (BaseEffect eff in StatManager.Instance.ActiveConsumableEffects)
        {
            BaseDiceEffect diceEff = eff as BaseDiceEffect;
            if (diceEff != null && !diceEff.RequiresAsyncResolution)
                result = diceEff.ModifyRoll(result, ctx);
        }

        return result;
    }

    // -------------------------------------------------------------------------
    // PUBLIC ACCESS
    // -------------------------------------------------------------------------

    public (int baseRoll, int finalRoll)? GetRollInfo(ItemSlot slot)
    {
        if (rollHistory.TryGetValue(slot, out (int baseRoll, int finalRoll) info))
            return info;

        return null;
    }

    public bool HasSlotRolledThisTurn(ItemSlot slot)
    {
        return rolledThisTurn.Contains(slot);
    }

    public bool HaveAllActiveDiceRolled()
    {
        foreach (ItemSlot slot in InventoryManager.Instance.ActiveDice.GetNonEmptySlots())
        {
            if (!rolledThisTurn.Contains(slot))
                return false;
        }
        return true;
    }

    public int GetTotalRoll()
    {
        int total = 0;

        foreach (ItemSlot slot in InventoryManager.Instance.ActiveDice.GetNonEmptySlots())
        {
            if (rollHistory.TryGetValue(slot, out (int baseRoll, int finalRoll) info))
                total += info.finalRoll;
        }

        return total;
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
    // FINALIZATION
    // -------------------------------------------------------------------------

    private void FinalizeRoll(int finalRoll)
    {
        Debug.Log("Final roll result: " + finalRoll);
        StatManager.Instance.OnDiceFinalResult(finalRoll);

        if (playerMovement != null)
            playerMovement.StartMoving();
    }

    public void ResetDiceTurnState()
    {
        rolledThisTurn.Clear();
        rollHistory.Clear();
    }
}
