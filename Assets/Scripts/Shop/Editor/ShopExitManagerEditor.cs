using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ShopExitManager))]
public class ShopExitManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ShopExitManager manager = (ShopExitManager)target;

        GUILayout.Space(10);
        GUILayout.Label("Debug Shop Controls", EditorStyles.boldLabel);

        if (GUILayout.Button("Enter Shop (Play Mode Only)"))
        {
            if (Application.isPlaying)
                manager.EnterShop();
            else
                Debug.LogWarning("Solo funciona en Play Mode.");
        }

        if (GUILayout.Button("Exit Shop (Play Mode Only)"))
        {
            if (Application.isPlaying)
                manager.ConfirmExit();
            else
                Debug.LogWarning("Solo funciona en Play Mode.");
        }

        GUILayout.Space(10);
        GUILayout.Label("Editor Preview", EditorStyles.boldLabel);

        if (GUILayout.Button("Preview Shop in Scene"))
        {
            manager.EditorPreviewShop();
        }
    }
}
