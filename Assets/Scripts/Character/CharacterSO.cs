using UnityEngine;

/*
 * CharacterSO (Cup Character)
 * ---------------------------
 * Represents a playable cup-character.
 * Each cup defines:
 * - Visual identity (name, icon, color)
 * - Spawn point
 * - Effects (dice, passive, or any other BaseEffect)
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

    [Header("Effects")]
    [Tooltip("All effects applied while this cup is active (dice, passive, etc.).")]
    public BaseEffect[] effects;

    [Header("Special Cup Behaviors")]
    public bool isBasicCup;            // Basic Cup
    public bool hasRandomBonus;        // Random Bonus Cup (10% extra effect)
    public bool avoidsBadTileEvery3;   // Enchanted Cup (avoids bad tile every 3 turns)
    public bool isMetalCup;            // Metal Cup (+1 to d4/d6)
}
