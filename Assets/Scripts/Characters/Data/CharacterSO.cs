using UnityEngine;

[CreateAssetMenu(fileName = "NewCupCharacter", menuName = "Game/Cup Character")]
public class CharacterSO : ScriptableObject
{
    [Header("Identity")]
    public string characterID;          // Unique internal ID used for saving, unlocking and referencing
    public string characterName;        // Display name shown to the player
    [TextArea] public string description; // Character description shown in UI

    [Header("Unlock Info")]
    public string unlockHint;           // Hint shown when the character is locked
    public Sprite icon;                 // Icon used in character selection and UI

    [Header("Visuals")]
    public Color characterColor = Color.white; // Main color applied to the cup and UI elements
    public bool applyColor = true;             // If true, the characterColor is applied to the cup

    public bool applyTileColor;                // If true, the tile receives the characterColor
    public Material tileMaterial;              // Specific material inside the tile that gets recolored

    [Header("Cup Materials")]
    public Material[] cupLightMaterials;   // Materials that receive the light color of the palette
    public Material[] cupDarkMaterials;    // Materials that receive the dark color of the palette

    [Header("Prefabs")]
    public GameObject cupPrefab;           // 3D prefab for the cup used on the board
    public GameObject tilePrefab;          // 3D prefab for the tile used on the board

    [Header("Spawn")]
    public string spawnPointName;          // Name of the spawn point where the character starts
    public int tileSpotIndex;              // Index of the tile spot used for initial placement

    [Header("Effects")]
    public BaseEffect[] effects;           // Passive effects granted by this character

    [Header("Starting Dice")]
    public DiceSO startingDice;            // Dice the character starts the game with
}
