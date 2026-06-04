using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public static class ListScriptsGrouped
{
    [MenuItem("Tools/List Scripts Grouped")]
    public static void ListScripts()
    {
        string root = "Assets/Scripts";

        if (!Directory.Exists(root))
        {
            Debug.Log("No existe la carpeta Assets/Scripts");
            return;
        }

        string[] files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);

        Dictionary<string, List<string>> groups = new Dictionary<string, List<string>>();

        foreach (string file in files)
        {
            string folder = Path.GetDirectoryName(file).Replace("\\", "/");

            if (!groups.ContainsKey(folder))
                groups[folder] = new List<string>();

            groups[folder].Add(Path.GetFileName(file));
        }

        Debug.Log("=== SCRIPTS AGRUPADOS POR CARPETA ===");

        foreach (var group in groups)
        {
            Debug.Log("Carpeta: " + group.Key);

            foreach (var script in group.Value)
                Debug.Log("   - " + script);
        }

        Debug.Log("=== FIN ===");
    }
}
