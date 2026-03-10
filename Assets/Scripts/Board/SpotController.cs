using UnityEngine;
using System.Linq;

/*
 * SpotController
 * --------------
 * Responsible for:
 * - Assigning random spot types at the start of the game
 * - Assigning checkpoint spots based on a selected color pattern
 * - Providing helper methods to access spots by index or order
 *
 * Notes:
 * - Checkpoints are always forced to be Neutral type
 * - GoodSpot() and BadSpot() return 1 or 2
 *   Movement triggers effects only when the result is 1
 */
public class SpotController : MonoBehaviour
{
    private Spot[] spots;
    private Spot spot;

    private void Start()
    {
        spot = GetComponent<Spot>();

        // Find all spots in the scene
        spots = FindObjectsByType<Spot>(FindObjectsSortMode.None);

        // Assign random types to all spots
        AssignRandomTypes();

        // Assign checkpoint spots (color pattern 1 by default)
        AssignCheckpoints(1);
    }

    /*
     * AssignRandomTypes
     * -----------------
     * Assigns a random SpotType to every spot.
     * Checkpoints will later be overwritten to Neutral.
     */
    private void AssignRandomTypes()
    {
        foreach (Spot s in spots)
        {
            s.AssignType(Spot.SpotType.Bad);
        }
    }

    /*
     * AssignCheckpoints
     * -----------------
     * Marks specific indices as checkpoints based on the selected color pattern.
     * Checkpoints are always forced to be Neutral type.
     */
    private void AssignCheckpoints(int color)
    {
        int[] checkpoints;

        if (color == 1)
        {
            checkpoints = new int[] { 12, 22, 34, 46, 56, 68 };
        }
        else if (color == 2)
        {
            checkpoints = new int[] { 29, 39, 51, 63, 5, 17 };
        }
        else if (color == 3)
        {
            checkpoints = new int[] { 46, 56, 68, 12, 22, 34 };
        }
        else
        {
            checkpoints = new int[] { 63, 5, 17, 29, 39, 51 };
        }

        foreach (Spot s in spots)
        {
            if (System.Array.Exists(checkpoints, x => x == s.index))
            {
                s.checkpoint = true;
                s.AssignType(Spot.SpotType.Normal);
            }
        }
    }

    /*
     * RandomType
     * ----------
     * Returns a random SpotType.
     */
    private Spot.SpotType RandomType()
    {
        int value = Random.Range(0, System.Enum.GetValues(typeof(Spot.SpotType)).Length);
        return (Spot.SpotType)value;
    }

    /*
     * GetSpot
     * -------
     * Returns the Spot component attached to this GameObject.
     */
    public Spot GetSpot() { return spot; }

    /*
     * GetAllSpots
     * -----------
     * Returns all spots in the scene.
     */
    public Spot[] GetAllSpots() { return spots; }

    /*
     * GetSpotsOrdered
     * ----------------
     * Returns all spots ordered by their index.
     */
    public Spot[] GetSpotsOrdered()
    {
        return spots.OrderBy(s => s.index).ToArray();
    }

    /*
     * GetSpotByIndex
     * --------------
     * Returns a spot by its board index.
     */
    public Spot GetSpotByIndex(int index)
    {
        return spots.FirstOrDefault(s => s.index == index);
    }

    /*
     * GetSpotCount
     * ------------
     * Returns the total number of spots.
     */
    public int GetSpotCount()
    {
        return spots.Length;
    }

    /*
     * GoodSpot
     * --------
     * Returns 1 or 2.
     * Movement triggers GOOD effects only when the result is 1.
     */
    public static int GoodSpot()
    {
        return Random.Range(1, 3);
    }

    /*
     * BadSpot
     * -------
     * Returns 1 or 2.
     * Movement triggers BAD effects only when the result is 1.
     */
    public static int BadSpot()
    {
        return Random.Range(1, 3);
    }
}
