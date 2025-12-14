using UnityEngine;

public class LootRandomizer : MonoBehaviour
{
    [Header("Possible items this loot can become")]
    public ItemSO[] possibleItems;

    private Loot lootScript;

    void Start()
    {
        if (possibleItems.Length == 0)
        {
            Debug.LogWarning("No possible items assigned for LootRandomizer!");
            return;
        }

        // Pick a random item
        int index = Random.Range(0, possibleItems.Length);
        ItemSO chosenItem = possibleItems[index];

        // Assign the chosen item to the Loot script
        lootScript = GetComponent<Loot>();
        if (lootScript == null)
        {
            Debug.LogError("Loot component missing!");
            return;
        }

        lootScript.itemSO = chosenItem;

        // Spawn the 3D model
        lootScript.SpawnModel();
    }
}
