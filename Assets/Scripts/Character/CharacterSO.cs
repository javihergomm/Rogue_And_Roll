using UnityEngine;

/*
 * CharacterSO (Cup Character)
 * ---------------------------
 * Represents a playable cup-character.
 * Each cup defines:
 * - Visual identity (name, icon, color)
 * - Spawn point
 * - Dice effects (modify rolls)
 * - Passive effects (turn-based or movement-based)
 * - Special flags for unique cup behaviors
 */
[CreateAssetMenu(fileName = "NewCupCharacter", menuName = "Game/Cup Character")]
public class CharacterSO : ScriptableObject
{
    [Header("Identity")]
    public string characterID;
    public string characterName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Visuals")]
    public Color characterColor = Color.white;

    [Header("Spawn")]
    public string spawnPointName;

    [Header("Dice Effects")]
    [Tooltip("Effects applied to every dice roll while this cup is active.")]
    public BaseDiceEffect[] diceEffects;

    [Header("Passive Effects")]
    [Tooltip("Effects applied on turn start/end, movement, etc.")]
    public BasePassiveEffect[] passiveEffects;

    [Header("Special Cup Behaviors")]
    public bool isBasicCup;            // Cubilete Básico
    public bool hasRandomBonus;        // Cubilete del Azar (10% efecto extra)
    public bool avoidsBadTileEvery3;   // Cubilete Encantado (cada 3 turnos evita casilla mala)
    public bool isMetalCup;            // Cubilete Metálico (+1 a d4/d6)
}
