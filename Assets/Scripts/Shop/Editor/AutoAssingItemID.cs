using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class AutoAssignItemIDs
{
    // Diccionario: nombre actual del SO -> (nuevo nombre espanol, ID final)
    private static readonly Dictionary<string, (string newName, string id)> map =
        new()
    {
        // Dados
        { "D4", ("Dado D4", "item_dice_d4") },
        { "D6", ("Dado D6", "item_dice_d6") },
        { "D8", ("Dado D8", "item_dice_d8") },
        { "D20", ("Dado D20", "item_dice_d20") },

        // Permanentes
        { "Amuleto de Precision", ("Amuleto de Precision", "item_permanents_precision_amulet") },
        { "Precision Amulet", ("Amuleto de Precision", "item_permanents_precision_amulet") },

        { "Brujula del Destino", ("Brujula del Destino", "item_permanents_destiny_compass") },
        { "Destiny Compass", ("Brujula del Destino", "item_permanents_destiny_compass") },

        { "Carta Joker", ("Carta Joker", "item_permanents_joker_card") },
        { "Joker Card", ("Carta Joker", "item_permanents_joker_card") },

        { "Linterna Potenciadora", ("Linterna Potenciadora", "item_permanents_power_lantern") },
        { "Power Lantern", ("Linterna Potenciadora", "item_permanents_power_lantern") },

        { "Gafas Destruidas", ("Gafas Destruidas", "item_permanents_broken_glasses") },
        { "Broken Glasses", ("Gafas Destruidas", "item_permanents_broken_glasses") },

        { "Escudo de Salida", ("Escudo de Salida", "item_permanents_exit_shield") },
        { "Exit Shield", ("Escudo de Salida", "item_permanents_exit_shield") },

        // Consumibles
        { "Espejo Maldito", ("Espejo Maldito", "item_consumables_cursed_mirror") },
        { "Cursed Mirror", ("Espejo Maldito", "item_consumables_cursed_mirror") },

        { "Puente del Catan", ("Puente del Catan", "item_consumables_catan_bridge") },
        { "Catan Bridge", ("Puente del Catan", "item_consumables_catan_bridge") },

        { "Mapa Roto", ("Mapa Roto", "item_consumables_broken_map") },
        { "Broken Map", ("Mapa Roto", "item_consumables_broken_map") },

        { "Varita de Cambio", ("Varita de Cambio", "item_consumables_change_wand") },
        { "Change Wand", ("Varita de Cambio", "item_consumables_change_wand") },

        { "Trebol de 4 Hojas", ("Trebol de 4 Hojas", "item_consumables_four_leaf_clover") },
        { "Four Leaf Clover", ("Trebol de 4 Hojas", "item_consumables_four_leaf_clover") },

        { "Cofre Mortal", ("Cofre Mortal", "item_consumables_deadly_chest") },
        { "Deadly Chest", ("Cofre Mortal", "item_consumables_deadly_chest") },

        { "Pocion del Azar", ("Pocion del Azar", "item_consumables_luck_potion") },
        { "Luck Potion", ("Pocion del Azar", "item_consumables_luck_potion") },

        { "Incienso Maldito", ("Incienso Maldito", "item_consumables_cursed_incense") },
        { "Cursed Incense", ("Incienso Maldito", "item_consumables_cursed_incense") },

        { "Bendicion del Clerigo", ("Bendicion del Clerigo", "item_consumables_cleric_blessing") },
        { "Cleric Blessing", ("Bendicion del Clerigo", "item_consumables_cleric_blessing") },

        // Especiales
        { "Gato de 9 Vidas", ("Gato de 9 Vidas", "item_special_cat_9_lives") },
        { "Cat 9 Lives", ("Gato de 9 Vidas", "item_special_cat_9_lives") },

        { "Limo", ("Limo", "item_special_slime_even_only") },
        { "Slime Even Only", ("Limo", "item_special_slime_even_only") },

        { "Enfurecido Munchkin", ("Enfurecido Munchkin", "item_special_angry_munchkin") },
        { "Angry Munchkin", ("Enfurecido Munchkin", "item_special_angry_munchkin") }
    };

    [MenuItem("Tools/Asignar IDs y Renombrar Items")]
    public static void AssignIDsAndRename()
    {
        string[] guids = AssetDatabase.FindAssets("t:BaseItemSO");
        int updated = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            BaseItemSO item = AssetDatabase.LoadAssetAtPath<BaseItemSO>(path);

            if (item == null)
                continue;

            if (map.TryGetValue(item.name, out var data))
            {
                Undo.RecordObject(item, "Auto Assign ID");

                // Asignar ID
                item.itemID = data.id;

                // Renombrar ScriptableObject
                AssetDatabase.RenameAsset(path, data.newName);

                EditorUtility.SetDirty(item);
                updated++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Proceso completado. Items actualizados: " + updated);
    }
}
