using UnityEngine;

public class EnemyDice : MonoBehaviour
{
    // Returns a random value between 1 and 6
    public static int ThrowDice()
    {
        return Random.Range(1, 7);
    }
}
