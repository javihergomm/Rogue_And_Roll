using UnityEngine;

public class Spot : MonoBehaviour
{
    public int index;

    public SpotType type;

    public bool checkpoint;
    public enum SpotType
    {
        Good,
        Bad,
        Normal
    }
    public void AssignType(SpotType newtype)
    {
        type = newtype;
    }

    public SpotType getType()
    {
        return type;
    }
}
