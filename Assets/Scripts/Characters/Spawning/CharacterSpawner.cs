using UnityEngine;

/*
 * CharacterSpawner
 * ----------------
 * Handles spawning of the selected character's cup and tile.
 * Applies color palettes when required.
 * Registers the Movement component so the dice system can control movement.
 * Sets the initial board index so the character starts on the correct tile.
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
     * Spawns the selected character's cup and tile.
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
    }

    /*
     * Spawns the character's cup at the assigned spawn point.
     */
    private void SpawnCup(CharacterSO character)
    {
        if (character.cupPrefab == null)
        {
            Debug.LogError("CharacterSpawner: Character has no cupPrefab assigned.");
            return;
        }

        Transform spawnPoint = GameObject.Find(character.spawnPointName)?.transform;

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn point not found: " + character.spawnPointName);
            return;
        }

        currentCup = Instantiate(character.cupPrefab, spawnPoint.position, spawnPoint.rotation);

        if (character.applyColor)
            ApplyPalette(currentCup, character.spawnPointName);
    }

    /*
     * Spawns the character's tile at the indexed board spot.
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
    }

    /*
     * Registers the Movement component and assigns the initial board index.
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
            DiceRollManager.Instance.RegisterPlayerMovement(mov);

            // Assigns the initial board index after Movement has completed its Start() method.
            StartCoroutine(AssignInitialPosition(mov));
        }
        else
        {
            Debug.LogError("CharacterSpawner: No Movement component found in cup or tile.");
        }
    }

    /*
     * Waits one frame to ensure Movement.Start() has finished,
     * then sets the initial board index.
     */
    private System.Collections.IEnumerator AssignInitialPosition(Movement mov)
    {
        yield return null;

        mov.ActualPos = lastCharacter.tileSpotIndex;
    }

    /*
     * Applies a two-tone palette to the cup based on the spawn point name.
     */
    private void ApplyPalette(GameObject obj, string spawnName)
    {
        Color light = Color.white;
        Color dark = Color.white;

        spawnName = spawnName.ToLower();

        if (spawnName.Contains("red"))
        {
            light = HexToColor("#FF6A6A");
            dark = HexToColor("#C62828");
        }
        else if (spawnName.Contains("blue"))
        {
            light = HexToColor("#6AB0FF");
            dark = HexToColor("#1565C0");
        }
        else if (spawnName.Contains("green"))
        {
            light = HexToColor("#6AFF8A");
            dark = HexToColor("#2E7D32");
        }
        else if (spawnName.Contains("yellow"))
        {
            light = HexToColor("#FFE66A");
            dark = HexToColor("#F9A825");
        }

        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
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

    /*
     * Converts a hex color string into a Unity Color.
     */
    private Color HexToColor(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }
}
