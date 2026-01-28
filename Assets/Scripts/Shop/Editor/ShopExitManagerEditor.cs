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

        if (GUILayout.Button("Enter Shop"))
        {
            manager.EnterShop();
        }

        if (GUILayout.Button("Exit Shop"))
        {
            manager.ConfirmExit();
        }
    }
}
