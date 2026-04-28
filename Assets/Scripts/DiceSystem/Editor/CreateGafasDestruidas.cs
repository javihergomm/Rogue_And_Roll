using UnityEngine;
using UnityEditor;

public static class CreateGafasDestruidas
{
    [MenuItem("Tools/Create Gafas Destruidas")]
    public static void CreateItemAndEffect()
    {
        // Correct resource paths
        string effectFolder = "Assets/Resources/Effects/Passive";
        string itemFolder = "Assets/Resources/Items/Permanents";

        string effectPath = effectFolder + "/DoubleBadSpotEffect.asset";
        string itemPath = itemFolder + "/GafasDestruidas.asset";

        // Ensure folders exist
        EnsureFolder("Assets/Resources");
        EnsureFolder("Assets/Resources/Effects");
        EnsureFolder("Assets/Resources/Effects/Passive");
        EnsureFolder("Assets/Resources/Items");
        EnsureFolder("Assets/Resources/Items/Permanents");

        // 1. Create effect SO
        DoubleBadSpotEffect effect = ScriptableObject.CreateInstance<DoubleBadSpotEffect>();
        AssetDatabase.CreateAsset(effect, effectPath);
        Debug.Log("[Editor] Created effect: " + effectPath);

        // 2. Create item SO
        PermanentSO item = ScriptableObject.CreateInstance<PermanentSO>();

        // Assign public fields
        item.itemID = "item_permanent_gafas_destruidas";
        item.CannotBeUnequipped = true;

        // SerializedObject to assign private fields
        SerializedObject so = new SerializedObject(item);

        so.FindProperty("itemName").stringValue = "Gafas Destruidas";
        so.FindProperty("itemDescription").stringValue = "Duplica los efectos de las casillas malas.";
        so.FindProperty("polarity").enumValueIndex = (int)BaseItemSO.ItemPolarity.Negative;

        // Assign effect array
        SerializedProperty effectsProp = so.FindProperty("effects");
        effectsProp.arraySize = 1;
        effectsProp.GetArrayElementAtIndex(0).objectReferenceValue = effect;

        so.ApplyModifiedProperties();

        // Save item
        AssetDatabase.CreateAsset(item, itemPath);
        Debug.Log("[Editor] Created item: " + itemPath);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[Editor] Gafas Destruidas created and linked successfully.");
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
