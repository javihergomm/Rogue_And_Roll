using UnityEngine;

/*
 * ShopAnimationRelay
 * ------------------
 * Bridge between animation events and the shop logic.
 */
public class ShopAnimationRelay : MonoBehaviour
{
    public ShopExitManager shop;

    // Animation event: enter animation start
    public void OnEnterStart()
    {
        if (shop != null)
            shop.OnEnterStart();
    }

    // Animation event: enter animation end
    public void OnEnterEnd()
    {
        if (shop != null)
            shop.OnEnterEnd();
    }

    // Animation event: exit animation start
    public void OnExitStart()
    {
        if (shop != null)
            shop.OnExitStart();
    }

    // Animation event: exit animation end
    public void OnExitEnd()
    {
        if (shop != null)
            shop.OnExitEnd();
    }
}
