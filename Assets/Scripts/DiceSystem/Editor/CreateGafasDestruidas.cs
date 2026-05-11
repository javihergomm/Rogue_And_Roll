using UnityEngine;
using UnityEditor;

public static class CreateBendicionClerigo
{
    [MenuItem("Tools/Fix Bendicion del Clerigo (Efecto + Item)")]
    public static void CreateAll()
    {
        // ================================
        // 1) RUTAS
        // ================================
        string effectFolder = "Assets/Resources/Effects/ConsumableEffects";
        string itemFolder = "Assets/Resources/Items/Consumables";

        string effectPath = effectFolder + "/Efecto de Bendicion del Clerigo.asset";
        string itemPath = itemFolder + "/Bendicion del Clerigo.asset";

        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Effects");
        EnsureFolder(effectFolder);
        EnsureFolder("Assets/Resources/Items");
        EnsureFolder(itemFolder);

        // ================================
        // 2) BORRAR ASSETS ANTIGUOS
        // ================================
        DeleteIfExists(effectPath);
        DeleteIfExists(itemPath);

        // ================================
        // 3) CREAR EFECTO
        // ================================
        BendicionClerigoEffect effect = ScriptableObject.CreateInstance<BendicionClerigoEffect>();
        effect.name = "Efecto de Bendicion del Clerigo";

        AssetDatabase.CreateAsset(effect, effectPath);
        EditorUtility.SetDirty(effect);

        Debug.Log("[Editor] Efecto de Bendicion del Clerigo creado.");

        // ================================
        // 4) CREAR ITEM
        // ================================
        ConsumableSO item = ScriptableObject.CreateInstance<ConsumableSO>();
        item.name = "Bendicion del Clerigo";
        item.itemID = "BendicionClerigo";

        SerializedObject so = new SerializedObject(item);

        // Nombre y descripción
        so.FindProperty("itemName").stringValue = "Bendición del Clérigo";
        so.FindProperty("itemDescription").stringValue =
            "Durante X turnos, permite mover dos veces por turno.";

        // Asignar efecto en array privado "effects"
        SerializedProperty effectsProp = so.FindProperty("effects");
        effectsProp.arraySize = 1;
        effectsProp.GetArrayElementAtIndex(0).objectReferenceValue = effect;

        so.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(item, itemPath);
        EditorUtility.SetDirty(item);

        Debug.Log("[Editor] Item 'Bendicion del Clerigo' creado.");

        // ================================
        // 5) FINAL
        // ================================
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Editor] Bendicion del Clerigo (Efecto + Item) creado correctamente.");
    }

    // ================================
    // HELPERS
    // ================================
    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path);
            string folder = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static void DeleteIfExists(string path)
    {
        Object existing = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (existing != null)
        {
            AssetDatabase.DeleteAsset(path);
            Debug.Log("[Editor] Deleted old asset: " + path);
        }
    }
}
