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

    [Header("Prefabs")]
    public GameObject cupPrefab;   // Visible player cup
    public GameObject tilePrefab;  // Visible player tile

    [Header("Spawn")]
    public string spawnPointName;  // Cup spawn point
    public int tileSpotIndex;      // Tile spawn point

    [Header("Effects")]
    public BaseEffect[] effects;
}
