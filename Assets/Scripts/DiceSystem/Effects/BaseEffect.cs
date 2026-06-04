using UnityEngine;

/*
 * BaseEffect
 * ----------
 * Root class for all effect types.
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

    // ============================
    // CALLBACKS OPCIONALES
    // ============================

    // Turno del jugador
    public virtual void OnTurnStart() { }
    public virtual void OnTurnEnd() { }

    // Movimiento del jugador
    public virtual void OnMovementStart(Movement m) { }
    public virtual void OnMovementEnd(Movement m) { }

    public virtual void OnEnemyTurnStart(EnemyBase enemy) { }
    public virtual void OnEnemyTurnEnd(EnemyBase enemy) { }
}
