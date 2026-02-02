using UnityEngine;

/*
 * ShopItemPool
 * ------------
 * Contains only the items that can appear in shop pedestals.
 */
[CreateAssetMenu(fileName = "ShopItemPool", menuName = "Inventory/ShopItemPool")]
public class ShopItemPool : ScriptableObject
{
    public BaseItemSO[] items;
}
