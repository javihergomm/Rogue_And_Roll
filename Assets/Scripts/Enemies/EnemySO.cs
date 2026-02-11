using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Game/Enemy")]
public class EnemySO : ScriptableObject
{
    [Header("Identity")]
    public string enemyID;
    public string enemyName;

    [Header("Prefabs")]
    public GameObject cupPrefab;   // Visible enemy cup
    public GameObject tilePrefab;  // Visible enemy tile

    [Header("Effects")]
    public BaseEffect[] effects;   // Optional enemy effects

    [Header("Spawn Settings")]
    public string cupSpawnPointName;   // Cup spawn point
    public int tileSpotIndex;  // Tile spawn point

    [Header("Behavior")]
    public int lapsToActivate = 1;     // Laps required before activation
    public bool requiresPlayerLap = true; // Whether activation depends on player laps
}
