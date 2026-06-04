using UnityEngine;

[CreateAssetMenu(fileName = "NewCupCharacter", menuName = "Game/Cup Character")]
public class CharacterSO : ScriptableObject
{
    [Header("Identity")]
    public string characterID;
    public string characterName;
    [TextArea] public string description;
    [Header("Unlock Info")]
    public string unlockHint;
    public Sprite icon;
    

    [Header("Visuals")]
    public Color characterColor = Color.white;
    public bool applyColor = true;

    public bool applyTileColor;

    public Material tileMaterial;

    [Header("Cup Materials")]
    public Material[] cupLightMaterials;   // materiales que deben recibir el color claro
    public Material[] cupDarkMaterials;    // materiales que deben recibir el color oscuro


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
