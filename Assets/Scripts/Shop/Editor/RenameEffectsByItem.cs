using UnityEngine;
using UnityEditor;
using System.Reflection;

public static class RenameEffectsByItem
{
    [MenuItem("Tools/Renombrar Efectos segun Objetos")]
    public static void RenameEffects()
    {
        System.Type[] itemTypes = new System.Type[]
        {
            typeof(DiceSO),
            typeof(ConsumableSO),
            typeof(PermanentSO),
            typeof(LootBoxSO)
        };

        int renamed = 0;

        foreach (System.Type type in itemTypes)
        {
            string[] guids = AssetDatabase.FindAssets("t:" + type.Name);

            foreach (string guid in guids)
            {
                string itemPath = AssetDatabase.GUIDToAssetPath(guid);
                ScriptableObject itemSO = AssetDatabase.LoadAssetAtPath<ScriptableObject>(itemPath);

                if (itemSO == null)
                    continue;

                FieldInfo[] fields = type.GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    // Caso 1: campo es BaseEffect (uno solo)
                    if (typeof(BaseEffect).IsAssignableFrom(field.FieldType))
                    {
                        BaseEffect effect = field.GetValue(itemSO) as BaseEffect;
                        renamed += TryRenameEffect(effect, itemSO);
                    }

                    // Caso 2: campo es BaseEffect[]
                    if (field.FieldType.IsArray &&
                        typeof(BaseEffect).IsAssignableFrom(field.FieldType.GetElementType()))
                    {
                        BaseEffect[] effects = field.GetValue(itemSO) as BaseEffect[];

                        if (effects == null)
                            continue;

                        foreach (BaseEffect effect in effects)
                        {
                            renamed += TryRenameEffect(effect, itemSO);
                        }
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Renombrado completado. Efectos renombrados: " + renamed);
    }

    private static int TryRenameEffect(BaseEffect effect, ScriptableObject itemSO)
    {
        if (effect == null)
            return 0;

        string effectPath = AssetDatabase.GetAssetPath(effect);
        if (string.IsNullOrEmpty(effectPath))
            return 0;

        string itemName = GetItemName(itemSO);
        string newEffectName = "Efecto de " + itemName;

        AssetDatabase.RenameAsset(effectPath, newEffectName);
        return 1;
    }

    private static string GetItemName(ScriptableObject item)
    {
        PropertyInfo prop = item.GetType().GetProperty("ItemName",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        if (prop != null)
        {
            object value = prop.GetValue(item);
            if (value != null)
                return value.ToString();
        }

        return item.name;
    }
}
