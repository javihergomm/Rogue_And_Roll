using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * Unlocks
 * -------
 * Manages the list of unlocked items and characters.
 * Supports saving, loading, and checking unlock status.
 */
public static class Unlocks
{
    private static HashSet<string> unlocked = new();

    /*
     * Default items that start unlocked at the beginning of the game.
     */
    private static readonly string[] defaultUnlocked =
    {
        "character_amarillo",
        "character_rojo",
        "character_azul",
        "character_verde",
        "item_dice_d6",
        "item_lootbox_negative",
        "item_lootbox_positive",
        "item_permanents_precision_amulet",
        "item_consumables_deadly_chest",
        "item_consumables_cursed_incense",
        "item_consumables_broken_map",
        "item_permanents_joker_card",
        "item_permanents_broken_glasses",
        "item_special_slime_even_only",
        "item_permanent_gato_9_vidas"
    };

    /*
     * Returns true if the given ID is unlocked.
     */
    public static bool IsUnlocked(string id)
    {
        return unlocked.Contains(id);
    }

    /*
     * Unlocks an item and shows a popup if it was not unlocked before.
     */
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

    /*
     * Loads unlock data from PlayerPrefs.
     * If no data exists, initializes with default unlocked items.
     */
    public static void Load()
    {
        if (!PlayerPrefs.HasKey("unlock_data"))
        {
            unlocked = new HashSet<string>();

            foreach (var id in defaultUnlocked)
                unlocked.Add(id);

            Save();
            return;
        }

        var json = PlayerPrefs.GetString("unlock_data");
        var wrapper = JsonUtility.FromJson<Wrapper>(json);
        unlocked = new HashSet<string>(wrapper.ids);
    }

    /*
     * Saves the current unlock list to PlayerPrefs.
     */
    public static void Save()
    {
        var wrapper = new Wrapper(unlocked);
        var json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("unlock_data", json);
    }

    /*
     * Wrapper used for JSON serialization.
     */
    [Serializable]
    private class Wrapper
    {
        public List<string> ids;
        public Wrapper(HashSet<string> set) => ids = new List<string>(set);
    }

    /*
     * Resolves the category of an item based on its folder or asset path.
     * Uses caching to avoid repeated lookups.
     */
    public static class ItemCategoryResolver
    {
        private static Dictionary<string, string> cache = new();

        public static string GetCategory(BaseItemSO item)
        {
            if (item == null)
                return "???";

            if (cache.TryGetValue(item.name, out var cat))
                return cat;

            // Runtime category detection using Resources folder structure
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

    /*
     * Returns all unlocked IDs.
     */
    public static IEnumerable<string> GetAllUnlockedIDs()
    {
        return unlocked;
    }
}
