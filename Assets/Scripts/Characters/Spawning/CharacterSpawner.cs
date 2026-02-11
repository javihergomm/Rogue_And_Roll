using UnityEngine;

/*
 * CharacterSpawner
 * ----------------
 * Spawns the selected character cup at the correct spawn point.
 * If the character is basic (applyColor = true), it applies a palette
 * based on the spawn point name (red, blue, green, yellow).
 * Advanced characters keep their original materials.
 */
public class CharacterSpawner : MonoBehaviour
{
    public static CharacterSpawner Instance { get; private set; }

    private GameObject currentCharacter;

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

    public GameObject Spawn(CharacterSO character, GameObject prefab)
    {
        if (character == null || prefab == null)
        {
            Debug.LogError("CharacterSpawner: Missing character or prefab.");
            return null;
        }

        Transform spawnPoint = GameObject.Find(character.spawnPointName)?.transform;

        if (spawnPoint == null)
        {
            Debug.LogError("Spawn point not found: " + character.spawnPointName);
            return null;
        }

        // Remove previous character
        if (currentCharacter != null)
            Destroy(currentCharacter);

        // Instantiate new character
        currentCharacter = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        // Apply palette only to basic characters
        if (character.applyColor)
            ApplyPalette(currentCharacter, character.spawnPointName);

        return currentCharacter;
    }

    /*
     * ApplyPalette
     * ------------
     * Applies a two-tone palette (light/dark) depending on the spawn point name.
     * Material index 0 = light tone
     * Material index 1 = dark tone
     */
    private void ApplyPalette(GameObject obj, string spawnName)
    {
        // Determine palette based on spawn name
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
        else
        {
            Debug.LogWarning("No palette matched for spawn: " + spawnName);
            return;
        }

        // Apply palette to materials
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            Material[] mats = r.materials;

            for (int i = 0; i < mats.Length; i++)
            {
                if (i == 0) // light tone
                {
                    if (mats[i].HasProperty("_BaseColor"))
                        mats[i].SetColor("_BaseColor", light);
                }
                else if (i == 1) // dark tone
                {
                    if (mats[i].HasProperty("_BaseColor"))
                        mats[i].SetColor("_BaseColor", dark);
                }
            }

            r.materials = mats;
        }
    }

    private Color HexToColor(string hex)
    {
        Color c;
        ColorUtility.TryParseHtmlString(hex, out c);
        return c;
    }
}
