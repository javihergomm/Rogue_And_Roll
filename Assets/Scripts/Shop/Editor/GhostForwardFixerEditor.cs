using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GhostWander))]
public class GhostForwardFixerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);

        if (GUILayout.Button("Fix Forward (Mesh -> Root)"))
        {
            FixForward();
        }
    }

    private void FixForward()
    {
        GhostWander ghost = (GhostWander)target;
        Transform root = ghost.transform;

        // Buscar el mesh real dentro del prefab
        MeshFilter mesh = root.GetComponentInChildren<MeshFilter>();
        if (mesh == null)
        {
            Debug.LogWarning("No se encontró MeshFilter en hijos.");
            return;
        }

        Transform meshT = mesh.transform;

        Undo.RecordObject(root, "Fix Ghost Forward");
        Undo.RecordObject(meshT, "Fix Ghost Forward");

        // Guardar la rotación actual del mesh
        Quaternion meshRot = meshT.localRotation;

        // Poner el mesh a 0,0,0
        meshT.localRotation = Quaternion.identity;

        // Aplicar la rotación al root para que visualmente no cambie
        root.rotation *= meshRot;

        Debug.Log("Rotación corregida. El fantasma ahora mira hacia adelante (Z+).");
    }
}
