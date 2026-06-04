using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

public class LootBoxAutoCreator : EditorWindow
{
    [MenuItem("Tools/Auto-Generate LootBoxes")]
    public static void Open()
    {
        GetWindow<LootBoxAutoCreator>("LootBox Auto Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generador automático de LootBoxes", EditorStyles.boldLabel);

        if (GUILayout.Button("Crear y rellenar LootBoxes"))
            GenerateLootBoxes();
    }

    private void GenerateLootBoxes()
    {
        // 1. Buscar todos los items del proyecto
        string[] guids = AssetDatabase.FindAssets("t:BaseItemSO");
        List<BaseItemSO> allItems = new List<BaseItemSO>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BaseItemSO item = AssetDatabase.LoadAssetAtPath<BaseItemSO>(path);
            if (item != null)
                allItems.Add(item);
        }

        // 2. Separar por polaridad
        BaseItemSO[] positives = allItems
            .Where(i => i.Polarity == BaseItemSO.ItemPolarity.Positive)
            .ToArray();

        BaseItemSO[] negatives = allItems
            .Where(i => i.Polarity == BaseItemSO.ItemPolarity.Negative)
            .ToArray();

        Debug.Log($"[LootBoxAuto] Positivos: {positives.Length}, Negativos: {negatives.Length}");

        // 3. Crear lootboxes si no existen
        LootBoxSO positiveBox = LoadOrCreate("LootBox_Positive.asset", LootBoxSO.LootType.Positive);
        LootBoxSO negativeBox = LoadOrCreate("LootBox_Negative.asset", LootBoxSO.LootType.Negative);

        // 4. Rellenar listas
        AssignList(positiveBox, positives, true);
        AssignList(negativeBox, negatives, false);

        // 5. Asignar metadatos compartidos
        SetupMetadata(positiveBox,
            "Contenedor Sellado",
            "Un recipiente opaco cuyo contenido no puede identificarse. Solo al abrirlo descubrirás lo que guarda.",
            "item_lootbox_positive");

        SetupMetadata(negativeBox,
            "Contenedor Sellado",
            "Un recipiente opaco cuyo contenido no puede identificarse. Solo al abrirlo descubrirás lo que guarda.",
            "item_lootbox_negative");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[LootBoxAuto] LootBoxes generadas y rellenadas correctamente.");
    }

    private LootBoxSO LoadOrCreate(string filename, LootBoxSO.LootType type)
    {
        string path = "Assets/Resources/Items/LootBox/" + filename;

        LootBoxSO box = AssetDatabase.LoadAssetAtPath<LootBoxSO>(path);

        if (box == null)
        {
            box = ScriptableObject.CreateInstance<LootBoxSO>();
            AssetDatabase.CreateAsset(box, path);
            Debug.Log("[LootBoxAuto] Creada lootbox: " + filename);
        }

        // Forzar polaridad correcta
        box.ForcePolarity(type);

        return box;
    }

    private void AssignList(LootBoxSO box, BaseItemSO[] items, bool positive)
    {
        SerializedObject so = new SerializedObject(box);

        SerializedProperty list = so.FindProperty(positive ? "positiveItems" : "negativeItems");

        list.arraySize = items.Length;

        for (int i = 0; i < items.Length; i++)
        {
            list.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(box);

        Debug.Log($"[LootBoxAuto] {(positive ? "Positiva" : "Negativa")} rellenada con {items.Length} items.");
    }

    private void SetupMetadata(LootBoxSO box, string name, string desc, string id)
    {
        SerializedObject so = new SerializedObject(box);

        so.FindProperty("itemName").stringValue = name;
        so.FindProperty("itemDescription").stringValue = desc;
        so.FindProperty("itemID").stringValue = id;

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(box);
    }
}
