//using UnityEngine;

///*
// * DemonDiceEffect
// * ----------------
// * This effect is used by the Demon boss, who rolls three dice.
// * The lethal condition is met only if ALL three dice roll a value of 6.
// *
// * Each die reports its result through this effect. The effect stores
// * the results for the current turn, and once all three dice have rolled,
// * it checks whether all values are equal to 6. If so, the player is killed.
// *
// * This effect does not modify the roll result.
// */
//[CreateAssetMenu(fileName = "DemonDiceEffect", menuName = "Effects/Dice/Demon")]
//public class DemonDiceEffect : BaseDiceEffect
//{
//    private static int[] demonRolls = new int[3];
//    private static int rollCount = 0;

//    [SerializeField]
//    private int lethalFace = 6;

//    public override int ModifyRoll(int roll, DiceContext ctx)
//    {
//        // Store this die's result
//        if (rollCount < 3)
//        {
//            demonRolls[rollCount] = roll;
//            rollCount++;
//        }

//        // When all 3 dice have rolled, evaluate the lethal condition
//        if (rollCount == 3)
//        {
//            bool allSixes =
//                demonRolls[0] == lethalFace &&
//                demonRolls[1] == lethalFace &&
//                demonRolls[2] == lethalFace;

//            if (allSixes)
//            {
//                PlayerManager.Instance.KillPlayer();
//            }

//            // Reset for next Demon roll sequence
//            rollCount = 0;
//        }

//        return roll;
//    }
//}
