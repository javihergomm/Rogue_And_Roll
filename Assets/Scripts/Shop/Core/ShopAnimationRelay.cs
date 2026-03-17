using UnityEngine;

public class ShopAnimationRelay : MonoBehaviour
{
    public ShopExitManager shop;
    public void OnEnterStart()
    {
        if (shop != null)
            shop.OnEnterStart();
    }

    public void OnEnterEnd()
    {
        if (shop != null)
            shop.OnEnterEnd();
    }
    public void OnExitStart()
    {
        if (shop != null)
            shop.OnExitStart();
    }

    public void OnExitEnd()
    {
        if (shop != null)
            shop.OnExitEnd();
    }
}
