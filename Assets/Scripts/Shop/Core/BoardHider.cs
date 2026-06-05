using UnityEngine;
using System.Collections.Generic;

/*
 * BoardHider
 * ----------
 * Stores references to gameplay objects that must be hidden when entering the shop.
 * Objects register themselves dynamically (cups, tokens, dice, enemies).
 */
public class BoardHider : MonoBehaviour
{
    public static BoardHider Instance;

    [SerializeField] private List<GameObject> objectsToHide = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ShopExitManager shop = Object.FindAnyObjectByType<ShopExitManager>();
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
            if (obj == null)
                continue;

            // If the object contains Movement, hide only the visuals
            Movement mov = obj.GetComponentInChildren<Movement>();
            if (mov != null)
            {
                // Hide or show all renderers without disabling the GameObject
                Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
                foreach (var r in renderers)
                    r.enabled = show;

                continue;
            }

            // Purely visual objects can be fully enabled/disabled
            obj.SetActive(show);
        }
    }
}
