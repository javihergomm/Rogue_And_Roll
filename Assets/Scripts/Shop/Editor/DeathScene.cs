using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class SimulateDeathWindow : EditorWindow
{
    [MenuItem("Tools/Testing/Simular Muerte")]
    public static void Open()
    {
        GetWindow<SimulateDeathWindow>("Simular Muerte");
    }

    private void OnGUI()
    {
        GUILayout.Label("Simulacion de Muerte del Jugador", EditorStyles.boldLabel);
        GUILayout.Space(10);

        GUILayout.Label("Este boton destruye al jugador y carga la escena 'Muerte'.");
        GUILayout.Space(10);

        if (GUILayout.Button("Simular Muerte"))
        {
            SimulateDeath();
        }
    }

    private void SimulateDeath()
    {
        Movement[] all = GameObject.FindObjectsByType<Movement>(FindObjectsSortMode.None);

        Movement player = null;

        foreach (var m in all)
        {
            if (m != null && m.isPlayer)
            {
                player = m;
                break;
            }
        }

        if (player == null)
        {
            Debug.LogError("No se encontro al jugador en la escena.");
            return;
        }

        // Destruir jugador
        GameObject.DestroyImmediate(player.gameObject);

        // Cargar escena de muerte
        SceneManager.LoadScene("Muerte");

        Debug.Log("Simulacion de muerte ejecutada correctamente.");
    }
}
