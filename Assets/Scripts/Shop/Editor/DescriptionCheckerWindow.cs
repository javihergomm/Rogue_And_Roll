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

        foreach (var f in GetAllFields(so.GetType()))
        {
            foreach (var n in names)
            {
                if (f.Name == n && f.FieldType == typeof(string))
                {
                    foundField = f;
                    Debug.Log("[DESC-FIELD] " + so.name + " usa campo: " + f.Name);
                    return (string)f.GetValue(so);
                }
            }
        }

        Debug.LogWarning("[DESC-FIELD] " + so.name + " NO tiene campo de descripcion compatible.");
        return null;
    }

    private string GenerateDescription(ScriptableObject so)
    {
        Debug.Log("[GEN-DESC] Procesando: " + so.name + " (" + so.GetType().Name + ")");

        string raw = so.name.ToLower();
        string name = RemoveAccents(raw);

        // 1. PERSONAJES BASICOS (nombre = solo el color)
        if (so.GetType().Name.Contains("Character"))
        {
            string color = ExtractColorFromName(so.name);
            Debug.Log("[GEN-DESC] Character detectado. Color: " + color + " | name=" + name);

            if (name == color)
            {
                Debug.Log("[GEN-DESC] Personaje basico detectado: " + so.name);
                return "El cubilete clasico. Sin efectos especiales: solo tu suerte, tu ruta y tu estrategia. La experiencia mas pura del juego.";
            }
        }

        // 2. CONSUMIBLES (van ANTES que cubiletes para evitar colisiones como 'pocion del azar')
        if (name.Contains("pocion"))
            return "Multiplica el resultado del dado entre x2 y x5.";

        if (name.Contains("espejo"))
            return "Teletransporta a la casilla buena o entrada de tienda mas cercana.";

        if (name.Contains("catan"))
            return "Crea un atajo durante la ronda.";

        if (name.Contains("mapa"))
            return "Desorienta al jugador y oculta la tirada.";

        if (name.Contains("varita"))
            return "Convierte una luckbox en su efecto opuesto.";

        if (name.Contains("trebol"))
            return "Convierte una casilla negativa en una luckbox.";

        if (name.Contains("cofre"))
            return "Intercambia una casilla normal por una mala.";

        if (name.Contains("incienso"))
            return "Impide moverse durante varios turnos.";

        if (name.Contains("clerigo"))
            return "Permite mover dos veces durante varios turnos.";

        // 3. LOOTBOXES (antes que cubiletes)
        if (so is LootBoxSO)
        {
            Debug.Log("[GEN-DESC] Lootbox detectada: " + so.name);

            if (name.Contains("positiva"))
                return "Lootbox positiva que contiene objetos beneficiosos.";

            if (name.Contains("negativa"))
                return "Lootbox negativa que contiene objetos peligrosos.";

            return "Lootbox que otorga un objeto aleatorio segun su polaridad.";
        }

        // 4. PERMANENTES
        if (name.Contains("precision"))
            return "Aumenta todas las tiradas en +1.";

        if (name.Contains("brujula"))
            return "Permite elegir entre dos resultados posibles del dado.";

        if (name.Contains("joker"))
            return "Reduce el rango del dado a la mitad de su valor inferior.";

        if (name.Contains("linterna"))
            return "Duplica el efecto de casillas positivas.";

        if (name.Contains("gafas"))
            return "Duplica el efecto de casillas peligrosas.";

        if (name.Contains("escudo"))
            return "Anula el efecto de la proxima casilla mala y se consume.";

        // 5. ESPECIALES
        if (name.Contains("gato"))
            return "Otorga una vida extra.";

        if (name.Contains("limo"))
            return "Fuerza que todas las tiradas sean pares.";

        // 6. CUBILETES (van al final porque son los mas genericos)
        if (name.Contains("metalico"))
            return "Un cubilete reforzado que estabiliza las tiradas pequenas. Aporta +1 a los dados d4 y d6, ideal para rutas seguras y movimientos controlados.";

        if (name.Contains("encantado"))
            return "Un cubilete imbuido con magia protectora. Cada 3 turnos, tu proximo movimiento ignora por completo la siguiente casilla mala que pises.";

        if (name.Contains("azar"))
            return "Un cubilete impredecible que altera el destino. Cada tirada tiene un 10% de activar un efecto extra: casilla estrategica, duplicar movimiento u otros eventos especiales.";

        if (name.Contains("basico"))
            return "El cubilete clasico. Sin efectos especiales: solo tu suerte, tu ruta y tu estrategia. La experiencia mas pura del juego.";

        // 7. NADA COINCIDE
        Debug.LogWarning("[GEN-DESC] No se encontro descripcion para: " + so.name);
        return "Sin descripcion.";
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
