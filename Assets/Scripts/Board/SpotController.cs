using System;
using System.Linq;
using UnityEngine;

/*
 * SpotController
 * --------------
 * Controls all board spots:
 * - Random type assignment (Normal / Good / Bad)
 * - Checkpoint assignment based on player color
 * - Stores global modifiers (Exit Shield, Lantern Boost, Clover)
 */
public class SpotController : MonoBehaviour
{
    public static SpotController Instance;

    // Cached array of all spots on the board
    private Spot[] spots;

    [Header("Probabilidades de tipo de casilla")]
    [Range(0, 100)] public int probNormal = 60;
    [Range(0, 100)] public int probGood = 20;
    [Range(0, 100)] public int probBad = 20;

    [Header("GOOD Spot Probabilities")]
    public int probGoodExtraSteps = 50;
    public int probGoodBlockEnemy = 50;
    public int probGoodLootBox = 0;

    [Header("BAD Spot Probabilities")]
    public int probBadNegativeSteps = 50;
    public int probBadBlockPlayer = 50;
    public int probBadLootBox = 0;

    // Exit Shield (Escudo de salida)
    public bool exitShieldActive = false;

    // Lantern Boost (Linterna potenciadora)
    public bool lanternBoostActive = false;

    // Trebol
    public bool cloverActive = false;
    public int savedBadSteps;
    public int savedBadBlock;
    public int savedBadLoot;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        /*
         * Updated Unity API:
         * FindObjectsByType<T>(FindObjectsInactive) replaces the deprecated overload
         * that used FindObjectsSortMode.
         * We only want active objects, so we use Exclude.
         */
        spots = FindObjectsByType<Spot>(FindObjectsInactive.Exclude);

        // Clear all checkpoints before assigning new ones
        foreach (Spot s in spots)
            s.checkpoint = false;

        AssignRandomTypes();
    }

    /*
     * Assigns a random type (Normal / Good / Bad) to each spot
     * based on the configured probability weights.
     */
    private void AssignRandomTypes()
    {
        foreach (Spot s in spots)
        {
            int roll = UnityEngine.Random.Range(0, 100);

            if (roll < probNormal)
                s.AssignType(Spot.SpotType.Normal);
            else if (roll < probNormal + probGood)
                s.AssignType(Spot.SpotType.Good);
            else
                s.AssignType(Spot.SpotType.Bad);
        }
    }

    /*
     * Assigns checkpoint spots depending on the player's color.
     * Checkpoints are fixed board positions.
     */
    public void AssignCheckpoints(int color)
    {
        // Clear previous checkpoints
        foreach (Spot s in spots)
            s.checkpoint = false;

        int[] checkpoints;

        if (color == 1)
            checkpoints = new int[] { 12, 22, 34, 46, 56, 68 };
        else if (color == 2)
            checkpoints = new int[] { 29, 39, 51, 63, 5, 17 };
        else if (color == 3)
            checkpoints = new int[] { 46, 56, 68, 12, 22, 34 };
        else
            checkpoints = new int[] { 63, 5, 17, 29, 39, 51 };

        foreach (Spot s in spots)
            s.checkpoint = checkpoints.Contains(s.index);
    }

    /*
     * Returns all spots ordered by their index.
     */
    public Spot[] GetSpotsOrdered()
    {
        return spots.OrderBy(s => s.index).ToArray();
    }

    /*
     * Returns a specific spot by index.
     */
    public Spot GetSpotByIndex(int index)
    {
        return spots.FirstOrDefault(s => s.index == index);
    }
}
