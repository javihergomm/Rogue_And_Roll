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

    private void OnValidate()
    {
        Debug.Log($"[COLORSPOT] {name} -> Left={leftPositionIndex}, Right={rightPositionIndex}");
    }
}


