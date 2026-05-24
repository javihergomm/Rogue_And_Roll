using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class ItemSOAutoFixer : EditorWindow
{
    [MenuItem("Tools/Fix Item ScriptableObjects (SAFE)")]
    public static void Open()
    {
        GetWindow<ItemSOAutoFixer>("Item SO Auto Fixer (SAFE)");
    }

    private Vector2 scroll;
    private List<string> report = new List<string>();

    private void OnGUI()
    {
        if (GUILayout.Button("Reparar todos los items"))
            FixAll();

        scroll = GUILayout.BeginScrollView(scroll);
        foreach (var line in report)
            GUILayout.Label(line);
        GUILayout.EndScrollView();
    }

    private void FixAll()
    {
        report.Clear();

        string[] guids = AssetDatabase.FindAssets("t:BaseItemSO");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BaseItemSO item = AssetDatabase.LoadAssetAtPath<BaseItemSO>(path);

            if (item == null)
                continue;

            if (item is DiceSO || item is LootBoxSO)
                continue;

            // DEBUG NORMALIZE
            Debug.Log("[DEBUG NORMALIZE] ItemName = '" + item.ItemName + "' -> Normalize = '" + Normalize(item.ItemName) + "'");

            string expectedType = GetExpectedType(item.ItemName);
            string actualType = item.GetType().Name;

            if (expectedType == "Unknown")
            {
                Log(item.ItemName + ": No esta en el catalogo oficial.");
                continue;
            }

            if (actualType != expectedType)
            {
                Log(item.ItemName + ": Corrigiendo tipo (" + actualType + " -> " + expectedType + ")");
                ReplaceAsset(item, expectedType, path);
            }
            else
            {
                Log(item.ItemName + ": Correcto (" + actualType + ")");
            }

            // Recargar asset corregido
            BaseItemSO fixedItem = AssetDatabase.LoadAssetAtPath<BaseItemSO>(path);

            if (fixedItem != null)
            {
                var so = new SerializedObject(fixedItem);
                so.FindProperty("polarity").enumValueIndex = (int)GetExpectedPolarity(fixedItem.ItemName);
                so.ApplyModifiedProperties();

                Log(fixedItem.ItemName + ": Polaridad corregida a " + fixedItem.Polarity);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void ReplaceAsset(BaseItemSO oldItem, string expectedType, string path)
    {
        string json = JsonUtility.ToJson(oldItem, true);

        BaseItemSO newItem =
            expectedType == "PermanentSO"
            ? ScriptableObject.CreateInstance<PermanentSO>()
            : ScriptableObject.CreateInstance<ConsumableSO>();

        JsonUtility.FromJsonOverwrite(json, newItem);

        AssetDatabase.DeleteAsset(path);
        AssetDatabase.CreateAsset(newItem, path);
    }

    private void Log(string msg)
    {
        report.Add(msg);
        Debug.Log("[ItemSOAutoFixer] " + msg);
    }

    private string Normalize(string s)
    {
        s = s.ToLower();

        // Eliminar acentos usando codigos unicode (ASCII-safe)
        s = s.Replace("\u00E1", "a"); // a con tilde
        s = s.Replace("\u00E9", "e"); // e con tilde
        s = s.Replace("\u00ED", "i"); // i con tilde
        s = s.Replace("\u00F3", "o"); // o con tilde
        s = s.Replace("\u00FA", "u"); // u con tilde

        s = s.Replace("\u00E0", "a"); // a grave
        s = s.Replace("\u00E8", "e"); // e grave
        s = s.Replace("\u00EC", "i"); // i grave
        s = s.Replace("\u00F2", "o"); // o grave
        s = s.Replace("\u00F9", "u"); // u grave

        s = s.Replace("\u00E4", "a"); // a dieresis
        s = s.Replace("\u00EB", "e"); // e dieresis
        s = s.Replace("\u00EF", "i"); // i dieresis
        s = s.Replace("\u00F6", "o"); // o dieresis
        s = s.Replace("\u00FC", "u"); // u dieresis

        s = s.Replace("\u00F1", "n"); // ñ

        // Quitar espacios y simbolos
        s = s.Replace(" ", "").Replace("-", "").Replace("_", "");

        return s;
    }


    private string GetExpectedType(string itemName)
    {
        string n = Normalize(itemName);

        // PERMANENTES
        if (n == "amuletodeprecision") return "PermanentSO";
        if (n == "brujuladeldestino") return "PermanentSO";
        if (n == "cartadeljoker") return "PermanentSO";
        if (n == "linternapotenciadora") return "PermanentSO";
        if (n == "gafasdestruidas") return "PermanentSO";
        if (n == "limo") return "PermanentSO";

        // CONSUMIBLES
        if (n == "espejomaldito") return "ConsumableSO";
        if (n == "puentedecatan") return "ConsumableSO";
        if (n == "maparoto") return "ConsumableSO";
        if (n == "varitadecambio") return "ConsumableSO";
        if (n == "trebolde4hojas") return "ConsumableSO";
        if (n == "cofremortal") return "ConsumableSO";
        if (n == "pociondelazar") return "ConsumableSO";
        if (n == "inciensomaldito") return "ConsumableSO";
        if (n == "bendiciondelclerigo") return "ConsumableSO";
        if (n == "escudodesalida") return "ConsumableSO";

        // ESPECIAL
        if (n == "gatode9vidas") return "ConsumableSO";

        return "Unknown";
    }

    private BaseItemSO.ItemPolarity GetExpectedPolarity(string itemName)
    {
        string n = Normalize(itemName);

        // ESPECIAL
        if (n == "limo") return BaseItemSO.ItemPolarity.Especial;
        if (n == "gatode9vidas") return BaseItemSO.ItemPolarity.Especial;

        // NEGATIVOS
        if (n == "cartadeljoker") return BaseItemSO.ItemPolarity.Negative;
        if (n == "gafasdestruidas") return BaseItemSO.ItemPolarity.Negative;
        if (n == "maparoto") return BaseItemSO.ItemPolarity.Negative;
        if (n == "cofremortal") return BaseItemSO.ItemPolarity.Negative;
        if (n == "inciensomaldito") return BaseItemSO.ItemPolarity.Negative;

        // POSITIVOS
        return BaseItemSO.ItemPolarity.Positive;
    }
}
