using UnityEngine;

public abstract class BaseDiceEffect : ScriptableObject
{
    // Modify the roll result before returning it
    public abstract int ModifyRoll(int roll, DiceContext ctx);
}
