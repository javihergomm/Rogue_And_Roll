using UnityEngine;

/*
 * LootBoxItemPool
 * ---------------
 * Contains only the items that can appear in loot boxes.
 * LootBoxSO filters these items by polarity at runtime.
 */
[CreateAssetMenu(fileName = "LootBoxItemPool", menuName = "Inventory/LootBoxItemPool")]
public class LootBoxItemPool : ScriptableObject
{
    public BaseItemSO[] items;
}
