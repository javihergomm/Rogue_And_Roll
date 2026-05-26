#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class NegativePolarityFinder : EditorWindow
{
    private Vector2 scroll;
    private List<string> negativeIDs = new List<string>();

    [MenuItem("Tools/Items/List Negative Polarity IDs")]
    public static void OpenWindow()
    {
        GetWindow<NegativePolarityFinder>("Negative Polarity IDs");
    }

    private void OnGUI()
    {
        GUILayout.Label("Find all items with polarity = Negative", EditorStyles.boldLabel);

        if (GUILayout.Button("Scan Project"))
        {
            Scan();
        }

        GUILayout.Space(10);

        if (negativeIDs.Count > 0)
        {
            GUILayout.Label("Found " + negativeIDs.Count + " items:", EditorStyles.boldLabel);

            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(300));
            foreach (var id in negativeIDs)
                GUILayout.Label(id);
            GUILayout.EndScrollView();

            if (GUILayout.Button("Copy to Clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = string.Join(",\n", negativeIDs);
                Debug.Log("Copied Negative IDs to clipboard.");
            }
        }
    }

    private void Scan()
    {
        negativeIDs.Clear();

        string[] guids = AssetDatabase.FindAssets("t:BaseItemSO");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BaseItemSO item = AssetDatabase.LoadAssetAtPath<BaseItemSO>(path);

            if (item == null)
                continue;

            // Usamos la propiedad publica Polarity
            if (item.Polarity == BaseItemSO.ItemPolarity.Negative)
            {
                negativeIDs.Add(item.itemID);
            }
        }

        Debug.Log("Scan complete. Found " + negativeIDs.Count + " negative items.");
    }
}
#endif
