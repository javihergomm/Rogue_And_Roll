using UnityEngine;
using System.Linq;

public class SpotController : MonoBehaviour
{
    private Spot[] spots;
    private Spot spot;
    void Start()
    {
        spot = GetComponent<Spot>();
        spots = FindObjectsOfType<Spot>();
        AssingRandomType();
        AssingCheckpoints(1);
    }

    void AssingRandomType()
    {
        foreach (Spot s in spots)
        {
            s.AssignType(RandomType());
        }
    }
    void AssingCheckpoints(int color)
    {
        int[] checkpoints = null;

        if (color == 1)
        {
            checkpoints = new int[] { 12, 22, 34, 46, 56, 68 };
        }
        else if (color == 2) {

            checkpoints = new int[] { 29, 39, 51, 63, 5, 17 };

        }else if (color == 3)
        {
            checkpoints = new int[] { 46, 56, 68, 12, 22, 34 };

        }else
        {
            checkpoints = new int[] { 63, 5, 17, 29, 39, 51 };
        }



            foreach (Spot spot in spots)
            {
                if (System.Array.Exists(checkpoints, x => x == spot.index))
                {
                    spot.checkpoint = true;
                }
            }
    }

    Spot.SpotType RandomType()
    {
        int valor = Random.Range(0, System.Enum.GetValues(typeof(Spot.SpotType)).Length);
        return (Spot.SpotType)valor;
    }

    public Spot GetSpot() { return spot; }
    public Spot[] GetAllSpots() { return spots; }

    // Devuelve los spots ordenados por índice
    public Spot[] GetSpotsOrdered()
    {
        return spots.OrderBy(s => s.index).ToArray();
    }

    // Devuelve un spot por índice
    public Spot GetSpotByIndex(int index)
    {
        return spots.FirstOrDefault(s => s.index == index);
    }

    // Devuelve el número total de spots
    public int GetSpotCount()
    {
        return spots.Length;
    }

    public static int GoodSpot()
    {
        int number = Random.Range(1, 3);

        return number;
    }
    public static int BadSpot()
    {
        int number = Random.Range(1, 3);

        return number;
    }
    
}
