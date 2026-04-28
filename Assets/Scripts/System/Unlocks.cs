using System;
using System.Collections.Generic;
using UnityEngine;

public static class Unlocks
{
    private static HashSet<string> unlocked = new();

    // IDs que empiezan desbloqueados
    private static readonly string[] defaultUnlocked =
    {
        "char_basic_1",
        "char_basic_2",
        "char_basic_3",
        "char_basic_4",
        "item_dice_d6",
        "item_lootbox_lootbox",
        "item_permanents_precision_amulet"
    };

    public static bool IsUnlocked(string id)
    {
        return unlocked.Contains(id);
    }

    public static void Unlock(string id)
    {
        if (unlocked.Add(id))
        {
            Save();

            BaseItemSO item = InventoryManager.Instance.GetItemSO(id);

            string name = item != null ? item.ItemName : id;
            string category = item != null ? ItemCategoryResolver.GetCategory(item) : "???";

            PopupHelpers.ShowUnlockPopup($"{name} ({category})");
        }
    }


    public static void Load()
    {
        if (!PlayerPrefs.HasKey("unlock_data"))
        {
            unlocked = new HashSet<string>();

            // Agregar los desbloqueados por defecto
            foreach (var id in defaultUnlocked)
                unlocked.Add(id);

            Save();
            return;
        }

        var json = PlayerPrefs.GetString("unlock_data");
        var wrapper = JsonUtility.FromJson<Wrapper>(json);
        unlocked = new HashSet<string>(wrapper.ids);
    }

    public static void Save()
    {
        var wrapper = new Wrapper(unlocked);
        var json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("unlock_data", json);
    }

    [Serializable]
    private class Wrapper
    {
        public List<string> ids;
        public Wrapper(HashSet<string> set) => ids = new List<string>(set);
    }
    public static class ItemCategoryResolver
    {
        private static Dictionary<string, string> cache = new();

        public static string GetCategory(BaseItemSO item)
        {
            if (item == null)
                return "???";

            // Cache para evitar búsquedas repetidas
            if (cache.TryGetValue(item.name, out var cat))
                return cat;

#if UNITY_EDITOR
            // EDITOR: ruta real del asset
            string path = UnityEditor.AssetDatabase.GetAssetPath(item);

            if (path.Contains("/Dice/"))
                return cache[item.name] = "Dado";

            if (path.Contains("/Consumables/"))
                return cache[item.name] = "Consumibles";

            if (path.Contains("/Permanents/"))
                return cache[item.name] = "Permanentes";

            if (path.Contains("/LootBox/"))
                return cache[item.name] = "Especial";
#endif

            // BUILD: buscar por carpeta en Resources usando itemID (no ItemName)
            string[] folders = { "Dice", "Consumables", "Permanents", "LootBox" };

            foreach (var folder in folders)
            {
                var loaded = Resources.Load<BaseItemSO>("Resources/Items/" + folder + "/" + item.itemID);
                if (loaded != null)
                {
                    string result = folder switch
                    {
                        "Dice" => "Dado",
                        "Consumables" => "Consumibles",
                        "Permanents" => "Permanentes",
                        "LootBox" => "Especial",
                        _ => folder
                    };

                    cache[item.name] = result;
                    return result;
                }
            }

            return "???";
        }

    }
    public static IEnumerable<string> GetAllUnlockedIDs()
    {
        return unlocked;
    }


#if UNITY_EDITOR
    [UnityEditor.MenuItem("Game/Reset Unlocks")]
    public static void ResetUnlocks()
    {
        PlayerPrefs.DeleteKey("unlock_data");
        unlocked.Clear();

        foreach (var id in defaultUnlocked)
            unlocked.Add(id);

        Save();

        Debug.Log("Unlocks reseteados para pruebas.");
    }
#endif

}
