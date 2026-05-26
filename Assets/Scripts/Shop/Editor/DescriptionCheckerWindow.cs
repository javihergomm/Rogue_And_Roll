using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

public class DescriptionAutoFill : EditorWindow
{
    private Vector2 scroll;

    private class SOInfo
    {
        public ScriptableObject asset;
        public string name;
        public string type;
        public string description;
        public bool hasDescription;
    }

    private List<SOInfo> results = new List<SOInfo>();

    [MenuItem("Tools/Description Auto-Fill")]
    public static void Open()
    {
        GetWindow<DescriptionAutoFill>("Description Auto-Fill");
    }

    private void OnGUI()
    {
        GUILayout.Label("Description Auto-Fill", EditorStyles.boldLabel);
        GUILayout.Label("Escanea ScriptableObjects dentro de Assets/Resources y rellena descripciones y stats.", EditorStyles.wordWrappedLabel);

        if (GUILayout.Button("Escanear y Rellenar"))
            ScanAndFill();

        GUILayout.Space(10);

        scroll = GUILayout.BeginScrollView(scroll);

        foreach (var r in results)
        {
            GUI.color = r.hasDescription ? Color.green : Color.red;

            GUILayout.BeginVertical("box");
            GUI.color = Color.white;

            GUILayout.Label("Nombre: " + r.name, EditorStyles.boldLabel);
            GUILayout.Label("Tipo: " + r.type);
            GUILayout.Label("Descripcion: " + (r.hasDescription ? r.description : "<VACIA>"));

            if (GUILayout.Button("Seleccionar Asset"))
                Selection.activeObject = r.asset;

            GUILayout.EndVertical();
            GUILayout.Space(5);
        }

        GUILayout.EndScrollView();
    }

    private void ScanAndFill()
    {
        results.Clear();

        string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets/Resources" });

        List<string> filtered = new List<string>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            path = path.Replace("\\", "/").ToLower();

            if (path.StartsWith("assets/resources/"))
                filtered.Add(guid);
        }
        guids = filtered.ToArray();

        Debug.Log("=== DESCRIPTION AUTO-FILL (Resources Only) ===");

        Dictionary<string, List<ScriptableObject>> charactersByColor = new Dictionary<string, List<ScriptableObject>>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            if (so == null)
                continue;

            if (so.GetType().Name.Contains("Character"))
            {
                string color = ExtractColorFromName(so.name);

                if (!charactersByColor.ContainsKey(color))
                    charactersByColor[color] = new List<ScriptableObject>();

                charactersByColor[color].Add(so);
            }
        }

        Dictionary<string, ScriptableObject> baseCharacters = new Dictionary<string, ScriptableObject>();

        foreach (var kvp in charactersByColor)
        {
            string color = kvp.Key;
            List<ScriptableObject> list = kvp.Value;

            ScriptableObject chosen = null;

            foreach (var so in list)
            {
                string n = RemoveAccents(so.name.ToLower());
                if (n.Contains("basico") || n.Contains("basic"))
                {
                    chosen = so;
                    break;
                }
            }

            if (chosen == null)
            {
                chosen = list[0];
                foreach (var so in list)
                {
                    if (so.name.Length < chosen.name.Length)
                        chosen = so;
                }
            }

            baseCharacters[color] = chosen;
            Debug.Log("[BASE-CHAR] Color " + color + " => " + chosen.name);
        }

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            ScriptableObject so = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);

            if (so == null)
                continue;

            string desc = GetDescriptionField(so, out FieldInfo field);

            if (field == null)
                continue;

            bool isCharacter = so.GetType().Name.Contains("Character");

            string auto = GenerateDescription(so);
            field.SetValue(so, auto);
            EditorUtility.SetDirty(so);

            Debug.Log((isCharacter ? "[AUTO-CHAR] " : "[AUTO-ITEM] ") + so.name + " => " + auto);

            desc = auto;

            if (isCharacter)
                AutoFillCharacterStats(so, baseCharacters);

            results.Add(new SOInfo
            {
                asset = so,
                name = so.name,
                type = so.GetType().Name,
                description = desc,
                hasDescription = !string.IsNullOrWhiteSpace(desc)
            });
        }

        AssetDatabase.SaveAssets();
        Debug.Log("=== FIN DEL AUTO-FILL ===");
    }

    private IEnumerable<FieldInfo> GetAllFields(System.Type t)
    {
        const BindingFlags flags =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance |
            BindingFlags.DeclaredOnly;

        while (t != null)
        {
            foreach (var f in t.GetFields(flags))
                yield return f;

            t = t.BaseType;
        }
    }
    private string GetDescriptionField(ScriptableObject so, out FieldInfo foundField)
    {
        foundField = null;

        string[] names =
        {
        "description",
        "itemDescription",
        "ItemDescription",
        "desc",
        "tooltip"
    };

        var fields = so.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var f in fields)
        {
            foreach (var n in names)
            {
                if (f.Name == n && f.FieldType == typeof(string))
                {
                    foundField = f;
                    return (string)f.GetValue(so);
                }
            }
        }

        return null;
    }

    private string GenerateDescription(ScriptableObject so)
    {
        Debug.Log("[GEN-DESC] Procesando: " + so.name + " (" + so.GetType().Name + ")");

        // Solo queremos cambiar descripciones de ciertos objetos negativos
        if (so is BaseItemSO baseItem)
        {
            switch (baseItem.itemID)
            {
                case "item_consumables_deadly_chest":
                    return "Un cofre antiguo con mecanismos poco comunes. Su contenido suele sorprender incluso a los jugadores mas experimentados.";

                case "item_consumables_cursed_incense":
                    return "Un incienso de aroma intenso que altera ligeramente la atmosfera del tablero. Su efecto puede cambiar el ritmo de la partida.";

                case "item_consumables_broken_map":
                    return "Un mapa desgastado que muestra rutas alternativas. No siempre es facil interpretarlo, pero puede revelar caminos inesperados.";

                case "item_permanents_joker_card":
                    return "Una carta comodin con reglas poco claras. Su influencia en el dado es peculiar y dificil de anticipar.";

                case "item_permanents_broken_glasses":
                    return "Unas gafas antiguas que distorsionan un poco la percepcion del tablero. A veces muestran detalles que pasan desapercibidos.";
            }
        }

        // Para todos los demas objetos NO cambiar la descripcion
        return null;
    }

    private string ExtractColorFromName(string name)
    {
        string lower = RemoveAccents(name.ToLower());

        if (lower.Contains("rojo")) return "rojo";
        if (lower.Contains("azul")) return "azul";
        if (lower.Contains("verde")) return "verde";
        if (lower.Contains("amarillo")) return "amarillo";
        if (lower.Contains("morado")) return "morado";
        if (lower.Contains("negro")) return "negro";
        if (lower.Contains("blanco")) return "blanco";

        return "desconocido";
    }

    private string RemoveAccents(string s)
    {
        string norm = s.Normalize(System.Text.NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();

        foreach (char c in norm)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
    }

    private void AutoFillCharacterStats(ScriptableObject so, Dictionary<string, ScriptableObject> baseCharacters)
    {
        string color = ExtractColorFromName(so.name);

        if (!baseCharacters.ContainsKey(color))
        {
            Debug.LogWarning("No se encontro personaje base para color: " + color);
            return;
        }

        ScriptableObject baseSO = baseCharacters[color];

        var fields = GetAllFields(so.GetType());
        var baseFields = GetAllFields(baseSO.GetType());

        string[] neverCopy =
        {
            "prefab",
            "icon",
            "tilematerial",
            "model",
            "avatar"
        };

        foreach (var f in fields)
        {
            string fname = f.Name.ToLower();

            if (fname.Contains("description"))
                continue;

            bool skip = false;
            foreach (var n in neverCopy)
            {
                if (fname.Contains(n))
                {
                    skip = true;
                    break;
                }
            }
            if (skip)
                continue;

            FieldInfo baseField = null;
            foreach (var bf in baseFields)
            {
                if (bf.Name == f.Name)
                {
                    baseField = bf;
                    break;
                }
            }

            if (baseField == null)
                continue;

            object baseValue = baseField.GetValue(baseSO);

            if (fname.Contains("characterid"))
            {
                string id = "character_" + RemoveAccents(so.name.ToLower().Replace(" ", "_"));
                f.SetValue(so, id);
                EditorUtility.SetDirty(so);
                continue;
            }

            if (fname.Contains("charactername"))
            {
                string pretty = so.name.Replace("_", " ");
                f.SetValue(so, pretty);
                EditorUtility.SetDirty(so);
                continue;
            }

            if (fname.Contains("spawnpointname"))
            {
                string spawn = "Spawn_" + char.ToUpper(color[0]) + color.Substring(1);
                f.SetValue(so, spawn);
                EditorUtility.SetDirty(so);
                continue;
            }

            if (fname.Contains("charactercolor"))
            {
                f.SetValue(so, baseValue);
                EditorUtility.SetDirty(so);
                continue;
            }

            object currentValue = f.GetValue(so);

            if (currentValue == null || currentValue.Equals(GetDefaultValue(f.FieldType)))
            {
                f.SetValue(so, baseValue);
                EditorUtility.SetDirty(so);
            }
        }
    }

    private object GetDefaultValue(System.Type t)
    {
        if (t.IsValueType)
            return System.Activator.CreateInstance(t);
        return null;
    }
}
