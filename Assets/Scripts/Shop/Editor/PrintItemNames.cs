using UnityEngine;
using UnityEditor;

public class PrintItemNames : EditorWindow
{
    [MenuItem("Tools/Debug/Print Item Names")]
    public static void Open()
    {
        GetWindow<PrintItemNames>("Item Name Printer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Imprimir ItemName de todos los items", EditorStyles.boldLabel);

        if (GUILayout.Button("Imprimir en consola"))
            PrintAll();
    }

    private void PrintAll()
    {
        string[] guids = AssetDatabase.FindAssets("t:BaseItemSO");

        Debug.Log("========== ITEM NAMES ==========");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BaseItemSO item = AssetDatabase.LoadAssetAtPath<BaseItemSO>(path);

            if (item == null)
                continue;

            Debug.Log($"Asset: {item.name}  |  ItemName: \"{item.ItemName}\"  |  Path: {path}");
        }

        Debug.Log("================================");
    }
}
