using UnityEngine;

/*
 * BaseEffect
 * ----------
 * Root class for all effect types.
 * Effects can be aligned as Positive or Negative so items/characters
 * can restrict which effects they accept.
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
