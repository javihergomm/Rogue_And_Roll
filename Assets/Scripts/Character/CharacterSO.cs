using UnityEngine;

/*
 * CharacterSO
 * -----------
 * Defines a playable character.
 * Contains display info, icon, spawn point name,
 * and a color applied to the cup's existing materials.
 */
[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character")]
public class CharacterSO : ScriptableObject
{
    [Tooltip("Unique ID used internally")]
    public string characterID;

    public string characterName;
    [TextArea] public string description;
    public Sprite icon;

    [Tooltip("Name of the empty where this character will spawn.")]
    public string spawnPointName;

    [Header("Cup Color")]
    [Tooltip("Color applied to the cup's existing materials.")]
    public Color characterColor = Color.white;

    [Header("Effects")]
    public BaseDiceEffect[] diceEffects;
    public BasePassiveEffect[] passiveEffects;
}
