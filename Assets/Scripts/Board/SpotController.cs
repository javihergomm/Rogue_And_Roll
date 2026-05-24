using System;
using System.Linq;
using UnityEngine;

public class SpotController : MonoBehaviour
{
    public static SpotController Instance;

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
        spots = FindObjectsByType<Spot>(FindObjectsSortMode.None);

        foreach (Spot s in spots)
            s.checkpoint = false;

        AssignRandomTypes();
    }


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

    public void AssignCheckpoints(int color)
    {
        
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

    public Spot[] GetSpotsOrdered()
    {
        return spots.OrderBy(s => s.index).ToArray();
    }

    public Spot GetSpotByIndex(int index)
    {
        return spots.FirstOrDefault(s => s.index == index);
    }
}
