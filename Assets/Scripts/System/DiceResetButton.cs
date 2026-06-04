using UnityEngine;

public class DiceResetButton : MonoBehaviour
{
    public void ResetAllDice()
    {
        
        DiceBoundary[] allDice = FindObjectsByType<DiceBoundary>();

        foreach (var dice in allDice)
            dice.ForceRespawn();

        Debug.Log("[DICE] Todos los dados han sido reseteados a su spawnPoint.");
    }
}
