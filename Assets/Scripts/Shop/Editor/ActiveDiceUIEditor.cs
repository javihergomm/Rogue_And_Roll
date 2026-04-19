using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ActiveDiceUI))]
public class ActiveDiceUIEditor : Editor
{
    private string testItemName = "item_d6";

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(10);
        GUILayout.Label("Debug en Play Mode", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Entra en Play Mode para usar estas opciones.", MessageType.Info);
            return;
        }

        testItemName = EditorGUILayout.TextField("Item a forzar:", testItemName);

        if (GUILayout.Button("Forzar dado en UI"))
        {
            ForceDice(testItemName);
        }

        if (GUILayout.Button("Refrescar UI"))
        {
            var ui = (ActiveDiceUI)target;
            var method = ui.GetType().GetMethod("RefreshPlayerDice", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method.Invoke(ui, null);
        }
    }

    private void ForceDice(string itemName)
    {
        var ui = (ActiveDiceUI)target;

        var resultsRootField = ui.GetType().GetField("resultsRoot", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var resultsRoot = (Transform)resultsRootField.GetValue(ui);

        var rowPrefabField = ui.GetType().GetField("rowPrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var rowPrefab = (GameObject)rowPrefabField.GetValue(ui);

        foreach (Transform child in resultsRoot)
            GameObject.DestroyImmediate(child.gameObject);

        var row = GameObject.Instantiate(rowPrefab, resultsRoot);

        var nameText = row.transform.Find("NameText").GetComponent<TMPro.TextMeshProUGUI>();
        var img = row.transform.Find("Image").GetComponent<UnityEngine.UI.Image>();
        var effText = row.transform.Find("EffectsText").GetComponent<TMPro.TextMeshProUGUI>();

        nameText.text = itemName + ": (debug)";
        img.enabled = false;
        effText.text = "Debug: fila generada manualmente";
    }
}
