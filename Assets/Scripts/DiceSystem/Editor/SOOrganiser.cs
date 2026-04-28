using UnityEngine;
using UnityEditor;
using System.IO;

public static class SOOrganizer
{
    [MenuItem("Tools/Organize ScriptableObjects")]
    public static void Organize()
    {
        Debug.Log("[Organizer] Starting organization...");

        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Skip non-Resources assets
            if (!path.Contains("Assets"))
                continue;

            ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (so == null)
                continue;

            string newPath = GetCorrectPath(so);
            if (newPath == null)
                continue;

            if (path != newPath)
            {
                EnsureFolder(Path.GetDirectoryName(newPath));
                Debug.Log("[Organizer] Moving: " + path + " -> " + newPath);
                AssetDatabase.MoveAsset(path, newPath);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Organizer] Finished organizing ScriptableObjects.");
    }

    private static string GetCorrectPath(ScriptableObject so)
    {
        // EFFECTS
        if (so is BasePassiveEffect)
            return "Assets/Resources/Effects/Passive/" + so.name + ".asset";

        if (so is BaseDiceEffect)
            return "Assets/Resources/Effects/Dice/" + so.name + ".asset";

        // ITEMS
        if (so is DiceSO)
            return "Assets/Resources/Items/Dice/" + so.name + ".asset";

        if (so is ConsumableSO)
            return "Assets/Resources/Items/Consumables/" + so.name + ".asset";

        if (so is PermanentSO)
            return "Assets/Resources/Items/Permanents/" + so.name + ".asset";

        if (so is LootBoxSO)
            return "Assets/Resources/Items/LootBox/" + so.name + ".asset";

        // Unknown type -> ignore
        return null;
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path))
            return;

        string parent = Path.GetDirectoryName(path);
        string folder = Path.GetFileName(path);

        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, folder);
    }
}
