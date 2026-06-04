using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ItemIntegrityChecker : EditorWindow
{
    private Vector2 scroll;

    private List<BaseItemSO> missingSprite = new List<BaseItemSO>();
    private List<BaseItemSO> missingPrefab = new List<BaseItemSO>();
    private List<BaseItemSO> missingBoth = new List<BaseItemSO>();

    [MenuItem("Tools/Items/Integrity Checker")]
    public static void Open()
    {
        GetWindow<ItemIntegrityChecker>("Item Integrity Checker");
    }

    private void OnGUI()
    {
        GUILayout.Label("Auditoria de Items", EditorStyles.boldLabel);
        GUILayout.Label("Detecta que items no tienen sprite, prefab 3D o ambos.");

        if (GUILayout.Button("Escanear Proyecto"))
            Scan();

        if (GUILayout.Button("Generar TXT con resultados"))
            GenerateTXT();

        GUILayout.Space(10);

        scroll = GUILayout.BeginScrollView(scroll);

        DrawSection("Falta Sprite", missingSprite);
        DrawSection("Falta Prefab 3D", missingPrefab);
        DrawSection("Faltan Ambos", missingBoth);

        GUILayout.EndScrollView();
    }

    private void DrawSection(string title, List<BaseItemSO> list)
    {
        if (list.Count == 0)
            return;

        GUILayout.BeginVertical("box");

        GUILayout.Label(title + " (" + list.Count + ")", EditorStyles.boldLabel);

        foreach (var item in list)
        {
            GUILayout.BeginHorizontal();

            GUILayout.Label(item.name);

            if (GUILayout.Button("Seleccionar", GUILayout.Width(100)))
                Selection.activeObject = item;

            GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
        GUILayout.Space(10);
    }

    private void Scan()
    {
        missingSprite.Clear();
        missingPrefab.Clear();
        missingBoth.Clear();

        string[] guids = AssetDatabase.FindAssets("t:BaseItemSO");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BaseItemSO item = AssetDatabase.LoadAssetAtPath<BaseItemSO>(path);

            if (item == null)
                continue;

            bool hasSprite = item.Icon != null;
            bool hasPrefab = item.Prefab3D != null;

            if (!hasSprite && !hasPrefab)
            {
                missingBoth.Add(item);
            }
            else
            {
                if (!hasSprite) missingSprite.Add(item);
                if (!hasPrefab) missingPrefab.Add(item);
            }
        }

        Debug.Log("[ItemIntegrityChecker] Escaneo completado.");
    }

    private void GenerateTXT()
    {
        string path = "Assets/ItemIntegrityReport.txt";

        using (StreamWriter writer = new StreamWriter(path, false))
        {
            writer.WriteLine("Item Integrity Report");
            writer.WriteLine("---------------------");
            writer.WriteLine("");

            writer.WriteLine("Falta Sprite (" + missingSprite.Count + ")");
            foreach (var item in missingSprite)
                writer.WriteLine(" - " + item.name);
            writer.WriteLine("");

            writer.WriteLine("Falta Prefab 3D (" + missingPrefab.Count + ")");
            foreach (var item in missingPrefab)
                writer.WriteLine(" - " + item.name);
            writer.WriteLine("");

            writer.WriteLine("Faltan Ambos (" + missingBoth.Count + ")");
            foreach (var item in missingBoth)
                writer.WriteLine(" - " + item.name);
            writer.WriteLine("");
        }

        AssetDatabase.Refresh();
        Debug.Log("[ItemIntegrityChecker] Archivo TXT generado en: " + path);
    }
}
