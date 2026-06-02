using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    // Nombre de la escena del juego
    public string gameSceneName = "GameScene";

    // Nombre de la escena del menu
    public string menuSceneName = "Menu";

    // Entrar al juego
    public void StartGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    // Volver al menu
    public void ReturnToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }

    // Salir de la aplicacion
    public void QuitGame()
    {
        // En el editor no funciona, pero no da error
        Application.Quit();
    }
}
