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
    }

    void AssingRandomType()
    {
        foreach (Spot s in spots)
        {
            s.AssignType(RandomType());
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
