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
    public bool applyColor = true;

    // Determines whether the tile should be recolored
    public bool applyTileColor;

    // The specific material inside the tile that should receive the palette color
    public Material tileMaterial;

    [Header("Prefabs")]
    public GameObject cupPrefab;
    public GameObject tilePrefab;

    [Header("Spawn")]
    public string spawnPointName;
    public int tileSpotIndex;

    [Header("Effects")]
    public BaseEffect[] effects;

    [Header("Starting Dice")]
    public DiceSO startingDice;
}
