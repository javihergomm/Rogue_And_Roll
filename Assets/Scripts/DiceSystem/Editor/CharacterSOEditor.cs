using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class CharacterSOScanner : EditorWindow
{
    private Vector2 scroll;

    [MenuItem("Tools/Scan CharacterSO")]
    public static void OpenWindow()
    {
        GetWindow<CharacterSOScanner>("CharacterSO Scanner");
    }

    private void OnGUI()
    {
        GUILayout.Space(10);
        GUILayout.Label("CharacterSO Scanner", EditorStyles.boldLabel);
        GUILayout.Label("Escanea todos los CharacterSO del proyecto y detecta problemas.", EditorStyles.wordWrappedLabel);

        GUILayout.Space(10);

        if (GUILayout.Button("Escanear CharacterSO"))
        {
            Scan();
        }

        GUILayout.Space(20);
    }

    private void Scan()
    {
        string[] guids = AssetDatabase.FindAssets("t:CharacterSO");

        List<CharacterSO> all = new List<CharacterSO>();
        Dictionary<string, List<CharacterSO>> byID = new Dictionary<string, List<CharacterSO>>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            CharacterSO so = AssetDatabase.LoadAssetAtPath<CharacterSO>(path);

            if (so == null)
                continue;

            all.Add(so);

            if (!byID.ContainsKey(so.characterID))
                byID[so.characterID] = new List<CharacterSO>();

            byID[so.characterID].Add(so);
        }

        Debug.Log("=== SCAN RESULTADOS ===");
        Debug.Log("Total CharacterSO encontrados: " + all.Count);

        Debug.Log("\n--- DUPLICADOS POR ID ---");
        foreach (var kvp in byID)
        {
            if (kvp.Value.Count > 1)
            {
                Debug.LogWarning("ID duplicado: " + kvp.Key);

                foreach (var so in kvp.Value)
                {
                    string path = AssetDatabase.GetAssetPath(so);
                    Debug.Log(" -> " + path);
                }
            }
        }

        Debug.Log("\n--- ESTADO DE UNLOCKS ---");
        foreach (var so in all)
        {
            bool unlocked = Unlocks.IsUnlocked(so.characterID);
            string path = AssetDatabase.GetAssetPath(so);

            if (!unlocked)
                Debug.LogWarning("BLOQUEADO: " + so.characterID + " -> " + path);
            else
                Debug.Log("DESBLOQUEADO: " + so.characterID + " -> " + path);
        }

        Debug.Log("\n--- CHARACTERSELECTMANAGER ---");

        CharacterSelectManager mgr = Object.FindFirstObjectByType<CharacterSelectManager>();

        if (mgr == null)
        {
            Debug.LogError("No se encontro CharacterSelectManager en la escena.");
            return;
        }

        CharacterSO[] arr = mgr.GetType()
            .GetField("characters", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(mgr) as CharacterSO[];

        HashSet<CharacterSO> used = new HashSet<CharacterSO>(arr);

        foreach (var so in all)
        {
            string path = AssetDatabase.GetAssetPath(so);

            if (!used.Contains(so))
                Debug.LogWarning("NO USADO EN SELECTOR: " + so.characterID + " -> " + path);
            else
                Debug.Log("USADO EN SELECTOR: " + so.characterID + " -> " + path);
        }

        Debug.Log("\n=== FIN DEL SCAN ===");
    }
}
