using UnityEngine;

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

    /*
     * SpawnCharacter
     * --------------
     * Instantiates the selected cup prefab at the correct spawn point.
     */
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

        // Apply color to all materials
        Renderer rend = currentCharacter.GetComponent<Renderer>();
        if (rend != null)
        {
            foreach (var mat in rend.materials)
                mat.color = character.characterColor;
        }

        return currentCharacter;
    }
}
