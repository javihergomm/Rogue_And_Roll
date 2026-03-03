using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShopPedestalRandomizer))]
public class ShopPedestalRandomizerEditor : Editor
{
    private BaseItemSO previewItem;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ShopPedestalRandomizer pedestal = (ShopPedestalRandomizer)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Editor Preview", EditorStyles.boldLabel);

        previewItem = (BaseItemSO)EditorGUILayout.ObjectField(
            "Item to Preview",
            previewItem,
            typeof(BaseItemSO),
            false
        );

        GUI.enabled = previewItem != null;
        if (GUILayout.Button("Preview Item on Pedestal"))
        {
            pedestal.EditorPreview(previewItem);
        }
        GUI.enabled = true;

        if (GUILayout.Button("Clear Preview"))
        {
            pedestal.EditorClearPreview();
        }

        if (GUI.changed)
            SceneView.RepaintAll();
    }
}
