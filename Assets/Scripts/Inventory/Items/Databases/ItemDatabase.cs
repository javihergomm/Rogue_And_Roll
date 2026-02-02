using UnityEngine;

/*
 * ItemDatabase
 * ------------
 * Global list of all items in the game.
 * Inventory uses this to resolve item names into ScriptableObjects.
 */
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private BaseItemSO[] allItems;

    public BaseItemSO[] AllItems => allItems;
}
