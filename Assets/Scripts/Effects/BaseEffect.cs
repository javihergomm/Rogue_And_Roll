using UnityEngine;

/*
 * BaseEffect
 * ----------
 * Root class for all effect types.
 * Every effect has an alignment (Positive or Negative) so objects
 * can restrict which effects they are allowed to contain.
 */
public enum EffectAlignment
{
    Positive,
    Negative
}

public abstract class BaseEffect : ScriptableObject
{
    [Header("Effect Alignment")]
    public EffectAlignment alignment;
}
