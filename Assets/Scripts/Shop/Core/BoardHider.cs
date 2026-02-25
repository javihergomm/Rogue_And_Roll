using UnityEngine;
using System.Collections.Generic;

/*
 * BoardHider
 * ----------
 * Stores references to gameplay objects that must be hidden when entering the shop.
 * Objects register themselves dynamically (cups, tokens, dice, enemies).
 * No tags, no name checks, no component guessing.
 */
public class BoardHider : MonoBehaviour
{
    public static BoardHider Instance;

    private List<GameObject> objectsToHide = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ShopExitManager shop = FindFirstObjectByType<ShopExitManager>();
        if (shop != null)
            shop.OnShopStateChanged += HandleShopState;
    }

    // Called by CharacterSpawner, EnemyBase, DiceRollManager, etc.
    public void RegisterObject(GameObject obj)
    {
        if (obj != null && !objectsToHide.Contains(obj))
            objectsToHide.Add(obj);
    }

    private void HandleShopState(bool inShop)
    {
        bool show = !inShop;

        foreach (var obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(show);
        }
    }
}
