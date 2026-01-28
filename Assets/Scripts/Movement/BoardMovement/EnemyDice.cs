using UnityEngine;

public class EnemyDice : MonoBehaviour
{
    public static int ThrowDice()
    {
        int rndnumber = Random.Range(1, 7);
        Debug.Log("el numero es: " + rndnumber);
        return rndnumber;
    }
}
