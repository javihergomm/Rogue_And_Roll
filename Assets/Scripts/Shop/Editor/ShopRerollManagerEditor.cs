using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShopRerollManager))]
public class ShopRerollManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ShopRerollManager reroll = (ShopRerollManager)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Editor Tools", EditorStyles.boldLabel);

#if UNITY_EDITOR
        if (GUILayout.Button("Preview Full Reroll"))
        {
            reroll.EditorForceReroll();
        }

        if (GUILayout.Button("Clear All Previews"))
        {
            reroll.EditorClearAll();
        }
#endif
    }
}
