//#if UNITY_EDITOR
//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(DiceRoller))]
//public class DiceRollerEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        DrawDefaultInspector();

//        DiceRoller roller = (DiceRoller)target;

//        GUILayout.Space(10);

//        if (GUILayout.Button("Print FaceMap"))
//        {
//            PrintFaceMap(roller);
//        }
//    }

//    private void PrintFaceMap(DiceRoller roller)
//    {
//        if (roller.EditorFaceMap == null || roller.EditorFaceMap.Count == 0)
//        {
//            Debug.Log("FaceMap is empty.");
//            return;
//        }

//        Debug.Log("---- PRINTING FACEMAP ----");

//        for (int i = 0; i < roller.EditorFaceMap.Count; i++)
//        {
//            FaceEntry f = roller.EditorFaceMap[i];

//            Debug.Log(
//                "Face " + f.value +
//                " | Normal: " + f.normal.ToString("F4") +
//                " | Center: " + f.center.ToString("F4") +
//                " | Checked: " + f.checkedByUser
//            );
//        }

//        Debug.Log("---- END FACEMAP ----");
//    }
//}
//#endif
