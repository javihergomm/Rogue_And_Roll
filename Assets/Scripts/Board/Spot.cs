using UnityEngine;

public class Spot : MonoBehaviour
{
    public SpotType type;

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
