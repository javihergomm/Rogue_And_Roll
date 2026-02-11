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

    [Header("Spawn")]
    public string spawnPointName;

    [Header("Effects")]
    public BaseEffect[] effects;

}
