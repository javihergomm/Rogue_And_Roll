using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/*
 * DiceRollerEditorHelper
 * ----------------------
 * Custom inspector for DiceRoller.
 * Lets you view and edit axis -> face mappings directly in the Unity Editor.
 */
[CustomEditor(typeof(DiceRoller))]
public class DiceRollerEditorHelper : Editor
{
    private DiceRoller roller;

    void OnEnable()
    {
        roller = (DiceRoller)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Face Mapping Editor", EditorStyles.boldLabel);

        if (roller.FaceMap == null)
        {
            EditorGUILayout.HelpBox("FaceMap is not initialized. Play the scene once or call InitFaceMap().", MessageType.Info);
        }
        else
        {
            // Show editable mappings
            List<Vector3> keys = new List<Vector3>(roller.FaceMap.Keys);
            foreach (var axis in keys)
            {
                int value = roller.FaceMap[axis];
                roller.FaceMap[axis] = EditorGUILayout.IntField(axis.ToString(), value);
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(roller);
        }
    }
}