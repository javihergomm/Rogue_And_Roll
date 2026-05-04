using UnityEngine;
using UnityEditor;

public static class CreateEspejoMaldito
{
    [MenuItem("Tools/Create Espejo Maldito")]
    public static void CreateItemAndEffect()
    {
        string effectFolder = "Assets/Resources/Effects/ConsumableEffects";
        string itemFolder = "Assets/Resources/Items/Consumables";

        string effectPath = effectFolder + "/MirrorTeleportEffect.asset";
        string itemPath = itemFolder + "/EspejoMaldito.asset";

        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Effects");
        EnsureFolder("Assets/Resources/Effects/ConsumableEffects");
        EnsureFolder("Assets/Resources/Items");
        EnsureFolder("Assets/Resources/Items/Consumables");

        // Crear efecto
        MirrorTeleportEffect effect = ScriptableObject.CreateInstance<MirrorTeleportEffect>();
        AssetDatabase.CreateAsset(effect, effectPath);
        Debug.Log("[Editor] Created effect: " + effectPath);

        // Crear item consumible
        ConsumableSO item = ScriptableObject.CreateInstance<ConsumableSO>();

        item.itemID = "item_consumable_espejo_maldito";

        SerializedObject so = new SerializedObject(item);

        so.FindProperty("itemName").stringValue = "Espejo Maldito";
        so.FindProperty("itemDescription").stringValue = "Teletransporta a la casilla positiva mas cercana o a la tienda.";
        so.FindProperty("polarity").enumValueIndex = (int)BaseItemSO.ItemPolarity.Positive;

        SerializedProperty effectsProp = so.FindProperty("effects");
        effectsProp.arraySize = 1;
        effectsProp.GetArrayElementAtIndex(0).objectReferenceValue = effect;

        so.ApplyModifiedProperties();

        AssetDatabase.CreateAsset(item, itemPath);
        Debug.Log("[Editor] Created item: " + itemPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Editor] Espejo Maldito created and linked successfully.");
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = System.IO.Path.GetDirectoryName(path);
            string folder = System.IO.Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
