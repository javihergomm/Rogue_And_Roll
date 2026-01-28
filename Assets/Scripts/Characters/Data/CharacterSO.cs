using UnityEngine;

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
    public BaseEffect[] effects;

    [Header("Special Cup Behaviors")]
    public bool isBasicCup;
    public bool hasRandomBonus;
    public bool avoidsBadTileEvery3;
    public bool isMetalCup;
}
