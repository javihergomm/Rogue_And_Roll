using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShopPedestalRandomizer))]
public class ShopPedestalRandomizerEditor : Editor
{
    private BaseItemSO previewItem;

    public override void OnInspectorGUI()
    {
        // Draw normal inspector
        DrawDefaultInspector();

        ShopPedestalRandomizer pedestal = (ShopPedestalRandomizer)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Editor Preview", EditorStyles.boldLabel);

        // Item selector
        previewItem = (BaseItemSO)EditorGUILayout.ObjectField(
            "Item to Preview",
            previewItem,
            typeof(BaseItemSO),
            false
        );

        // Preview button
        GUI.enabled = previewItem != null;
        if (GUILayout.Button("Preview Item on Pedestal"))
        {
            pedestal.EditorPreview(previewItem);
        }
        GUI.enabled = true;

        // Clear preview
        if (GUILayout.Button("Clear Preview"))
        {
            pedestal.EditorClearPreview();
        }

        // Force repaint so the scene updates instantly
        if (GUI.changed)
            SceneView.RepaintAll();
    }
}
