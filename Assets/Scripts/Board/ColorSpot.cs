using UnityEngine;

/*
 * ColorSpot
 * ---------
 * Defines the two board positions 
 * that the Bridge of Catan will connect.
 */
public class ColorSpot : MonoBehaviour
{
    [SerializeField] private int leftPositionIndex;
    [SerializeField] private int rightPositionIndex;

    public int LeftPositionIndex
    {
        get => leftPositionIndex;
        set => leftPositionIndex = value;
    }

    public int RightPositionIndex
    {
        get => rightPositionIndex;
        set => rightPositionIndex = value;
    }

}


