using UnityEngine;

/*
 * CharacterSpawner
 * ----------------
 * Handles spawning of the selected character's cup and tile,
 * applies color palettes when needed, registers the Movement
 * component, and sets the initial board position before any
 * movement logic begins.
 */
public class CharacterSpawner : MonoBehaviour
{
    public static CharacterSpawner Instance { get; private set; }

    private GameObject currentCup;
    private GameObject currentTile;
    private CharacterSO lastCharacter;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /*
     * Spawns the selected character and initializes all related systems.
     */
    public void Spawn(CharacterSO character)
    {
        if (character == null)
        {
            Debug.LogError("CharacterSpawner: Missing character.");
            return;
        }

        lastCharacter = character;

        SpawnCup(character);
        SpawnTile(character);
        RegisterMovementFromSpawnedObjects();

        if (character.startingDice != null)
            InventoryManager.Instance.AddStartingDice(character.startingDice);
    }

    /*
     * Spawns the character's cup at the assigned spawn point
     * and applies the palette if required.
     */
    private void SpawnCup(CharacterSO character)
    {
        if (character.cupPrefab == null)
        {
            Debug.LogError("CharacterSpawner: Character has no cupPrefab assigned.");
            return;
        }

        GameObject spawnObj = GameObject.Find(character.spawnPointName);
        if (spawnObj == null)
        {
            Debug.LogError("Spawn point not found: " + character.spawnPointName);
            return;
        }

        Transform spawnPoint = spawnObj.transform;
        currentCup = Instantiate(character.cupPrefab, spawnPoint.position, spawnPoint.rotation);

        if (BoardHider.Instance != null)
            BoardHider.Instance.RegisterObject(currentCup);

        if (character.applyColor)
        {
            (Color light, Color dark) = GetPalette(character.spawnPointName);
            ApplyPaletteToCup(currentCup, light, dark);
        }
    }

    /*
     * Spawns the character's tile at the specified board index
     * and recolors the selected material if enabled.
     */
    private void SpawnTile(CharacterSO character)
    {
        if (character.tilePrefab == null)
            return;

        Spot[] spots = Object.FindObjectsByType<Spot>(FindObjectsSortMode.None);
        System.Array.Sort(spots, (a, b) => a.index.CompareTo(b.index));

        if (character.tileSpotIndex < 0 || character.tileSpotIndex >= spots.Length)
        {
            Debug.LogError("Invalid tileSpotIndex for character: " + character.characterName);
            return;
        }

        Transform tilePoint = spots[character.tileSpotIndex].transform;
        currentTile = Instantiate(character.tilePrefab, tilePoint.position, tilePoint.rotation);

        if (BoardHider.Instance != null)
            BoardHider.Instance.RegisterObject(currentTile);

        if (character.applyTileColor && character.tileMaterial != null)
        {
            (Color light, _) = GetPalette(character.spawnPointName);
            ApplyPaletteToTile(currentTile, character.tileMaterial, light);
        }
    }

    /*
     * Registers the Movement component and assigns the initial board
     * position before any movement logic is executed.
     */
    private void RegisterMovementFromSpawnedObjects()
    {
        Movement mov = null;

        if (currentCup != null)
            mov = currentCup.GetComponentInChildren<Movement>();

        if (mov == null && currentTile != null)
            mov = currentTile.GetComponentInChildren<Movement>();

        if (mov != null)
        {
            // Register movement
            DiceRollManager.Instance.RegisterPlayerMovement(mov);
            
            // Set initial board position
            mov.ActualPos = lastCharacter.tileSpotIndex;

            if (mov.Positions != null &&
                mov.ActualPos >= 0 &&
                mov.ActualPos < mov.Positions.Length)
            {
                mov.transform.position = mov.Positions[mov.ActualPos].position;
            }
        }
        else
        {
            Debug.LogError("CharacterSpawner: No Movement component found in cup or tile.");
        }
    }

    // ---------------------------------------------------------
    //  PALETTE SYSTEM
    // ---------------------------------------------------------

    private (Color light, Color dark) GetPalette(string spawnName)
    {
        spawnName = spawnName.ToLower();

        if (spawnName.Contains("red"))
            return (HexToColor("#FF6A6A"), HexToColor("#C62828"));

        if (spawnName.Contains("blue"))
            return (HexToColor("#6AB0FF"), HexToColor("#1565C0"));

        if (spawnName.Contains("green"))
            return (HexToColor("#6AFF8A"), HexToColor("#2E7D32"));

        if (spawnName.Contains("yellow"))
            return (HexToColor("#FFE66A"), HexToColor("#F9A825"));

        return (Color.white, Color.white);
    }

    private void ApplyPaletteToCup(GameObject obj, Color light, Color dark)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            Material[] mats = r.materials;

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].HasProperty("_BaseColor"))
                    mats[i].SetColor("_BaseColor", i == 0 ? light : dark);
            }

            r.materials = mats;
        }
    }

    private void ApplyPaletteToTile(GameObject tile, Material targetMat, Color color)
    {
        string baseName = targetMat.name;

        Renderer[] renderers = tile.GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            Material[] mats = r.materials;

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i].name.StartsWith(baseName) && mats[i].HasProperty("_BaseColor"))
                    mats[i].SetColor("_BaseColor", color);
            }

            r.materials = mats;
        }
    }

    private Color HexToColor(string hex)
    {
        ColorUtility.TryParseHtmlString(hex, out Color c);
        return c;
    }
}
