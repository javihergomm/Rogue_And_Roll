using UnityEngine;

/*
 * ColorSpot
 * ---------
 * Simple marker component used to define
 * the two board positions that the Bridge of Catan
 * will connect.
 */
public class ColorSpot : MonoBehaviour
{
    [SerializeField] private int leftPositionIndex;
    [SerializeField] private int rightPositionIndex;

    public int LeftPositionIndex => leftPositionIndex;
    public int RightPositionIndex => rightPositionIndex;
}
