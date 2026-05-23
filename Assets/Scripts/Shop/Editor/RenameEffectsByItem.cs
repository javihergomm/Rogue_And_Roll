using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Text.RegularExpressions;

public static class RenameEffectsByItem
{
    [MenuItem("Tools/Renombrar Efectos segun Objetos (FULL)")]
    public static void RenameEffects()
    {
        System.Type[] itemTypes = new System.Type[]
        {
            typeof(DiceSO),
            typeof(ConsumableSO),
            typeof(PermanentSO),
            typeof(LootBoxSO),
            typeof(CharacterSO) // soporte para personajes
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
                    // Caso 1: campo unico BaseEffect
                    if (typeof(BaseEffect).IsAssignableFrom(field.FieldType))
                    {
                        BaseEffect effect = field.GetValue(itemSO) as BaseEffect;
                        renamed += RenameEffectAsset(effect, itemSO);
                    }

                    // Caso 2: array de BaseEffect
                    if (field.FieldType.IsArray &&
                        typeof(BaseEffect).IsAssignableFrom(field.FieldType.GetElementType()))
                    {
                        BaseEffect[] effects = field.GetValue(itemSO) as BaseEffect[];

                        if (effects == null)
                            continue;

                        foreach (BaseEffect effect in effects)
                        {
                            renamed += RenameEffectAsset(effect, itemSO);
                        }
                    }
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Renombrado completado. Efectos renombrados: " + renamed);
    }

    private static int RenameEffectAsset(BaseEffect effect, ScriptableObject itemSO)
    {
        if (effect == null)
            return 0;

        string newName = BuildEffectName(itemSO);

        string path = AssetDatabase.GetAssetPath(effect);

        // Si es sub-asset, migrarlo a asset propio
        if (AssetDatabase.IsSubAsset(effect))
        {
            string parentPath = path;
            string newPath = parentPath.Replace(".asset", "_" + newName + ".asset");

            BaseEffect clone = Object.Instantiate(effect);
            clone.name = newName;

            AssetDatabase.CreateAsset(clone, newPath);
            EditorUtility.SetDirty(clone);

            Object.DestroyImmediate(effect, true);

            Debug.Log("Sub-asset migrado y renombrado: " + newName);
            return 1;
        }

        // Asset normal
        AssetDatabase.RenameAsset(path, newName);
        effect.name = newName;
        EditorUtility.SetDirty(effect);

        Debug.Log("Asset renombrado: " + newName);
        return 1;
    }

    private static string BuildEffectName(ScriptableObject itemSO)
    {
        string rawName = GetItemName(itemSO);
        string clean = NormalizeName(rawName);

        // Si es personaje
        if (itemSO.GetType().Name.ToLower().Contains("character"))
        {
            string noColor = RemoveColor(clean);
            return "Efecto de Personaje " + noColor;
        }

        // Si es item normal
        return "Efecto de " + clean;
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

    private static string NormalizeName(string name)
    {
        name = Regex.Replace(name, @"\s+", " ");
        name = name.Trim();
        name = name.Replace("_", " ");
        return name;
    }

    private static string RemoveColor(string name)
    {
        string lower = name.ToLower();

        string[] colors = new string[]
        {
            "rojo", "azul", "verde", "amarillo", "morado", "negro", "blanco"
        };

        foreach (string c in colors)
        {
            if (lower.StartsWith(c))
            {
                string cut = name.Substring(c.Length).Trim();
                if (cut.Length > 0)
                    return cut;
            }
        }

        return name;
    }
}
