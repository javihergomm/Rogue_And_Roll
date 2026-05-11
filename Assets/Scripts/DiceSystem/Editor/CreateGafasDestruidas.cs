using UnityEngine;
using UnityEditor;

public static class CreateVaritaDeCambioItem
{
    [MenuItem("Tools/Fix Varita de Cambio (Item)")]
    public static void CreateItem()
    {
        string folder = "Assets/Resources/Items/Consumables";
        string itemPath = folder + "/Varita de Cambio.asset";

        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Items");
        EnsureFolder("Assets/Resources/Items/Consumables");

        // ---------------------------------------------------------
        // 1) ELIMINAR ITEM ANTIGUO SI EXISTE
        // ---------------------------------------------------------
        DeleteIfExists(itemPath);

        // ---------------------------------------------------------
        // 2) CARGAR EFECTO
        // ---------------------------------------------------------
        string effectPath = "Assets/Resources/Effects/ConsumableEffects/Efecto de Varita de Cambio.asset";
        ChangeLootBoxPolarityEffect effect = AssetDatabase.LoadAssetAtPath<ChangeLootBoxPolarityEffect>(effectPath);

        if (effect == null)
        {
            Debug.LogError("[Editor] No se encontro el efecto 'Efecto de Varita de Cambio'. Ejecuta primero el creador del efecto.");
            return;
        }

        // ---------------------------------------------------------
        // 3) CREAR ITEM NUEVO
        // ---------------------------------------------------------
        ConsumableSO item = ScriptableObject.CreateInstance<ConsumableSO>();
        item.name = "Varita de Cambio";

        // Asignar itemID directamente (es público)
        item.itemID = "VaritaCambio";

        // ---------------------------------------------------------
        // 4) Asignar nombre, descripción e icono mediante SerializedObject
        // ---------------------------------------------------------
        SerializedObject so = new SerializedObject(item);

        so.FindProperty("itemName").stringValue = "Varita de Cambio";
        so.FindProperty("itemDescription").stringValue = "Permite cambiar la polaridad de una lootbox del inventario.";
        // si quieres asignar icono:
        // so.FindProperty("icon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("ruta");

        // ---------------------------------------------------------
        // 5) Asignar el efecto en el array privado "effects"
        // ---------------------------------------------------------
        SerializedProperty effectsProp = so.FindProperty("effects");
        effectsProp.arraySize = 1;
        effectsProp.GetArrayElementAtIndex(0).objectReferenceValue = effect;

        so.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(item, itemPath);
        EditorUtility.SetDirty(item);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Editor] Item 'Varita de Cambio' creado correctamente.");
    }

    // ---------------------------------------------------------
    // HELPERS
    // ---------------------------------------------------------

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
