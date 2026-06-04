using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * MenuManager
 * -----------
 * Handles scene transitions for:
 *   - Starting the game
 *   - Returning to the main menu
 *   - Quitting the application
 *
 * Ensures the game always resumes normal time when changing scenes.
 */
public class MenuManager : MonoBehaviour
{
    public string gameSceneName = "Juego";
    public string menuSceneName = "Menu";

    /*
     * Loads the game scene and ensures time is running normally.
     */
    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    /*
     * Returns to the main menu and restores normal time.
     */
    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    /*
     * Quits the application.
     */
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
}
