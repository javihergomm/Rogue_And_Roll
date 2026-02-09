using UnityEngine;

public class SpotController : MonoBehaviour
{
    private Spot[] spots;
    private Spot spot;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
}
