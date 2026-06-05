using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * PauseButton
 * -----------
 * Handles the pause system using the game's popup UI.
 * When the pause menu opens:
 *   - The game is frozen (Time.timeScale = 0)
 *   - A popup appears with options for the player
 * When the player resumes:
 *   - The popup closes
 *   - The game continues normally
 */
public class PauseButton : MonoBehaviour
{
    private bool isPaused = false;

    /*
     * Opens the pause menu popup and freezes the game.
     */
    public void OpenPauseMenu()
    {
        if (isPaused)
            return;

        isPaused = true;
        Time.timeScale = 0f;

        var opciones = new System.Collections.Generic.List<PopupOption>
        {
            new PopupOption(
                "Continuar",
                () =>
                {
                    Time.timeScale = 1f;
                    isPaused = false;
                    OptionPopupManager.Instance.HidePopup();
                },
                isConfirm: true
            ),

            new PopupOption(
                "Volver al menu",
                () =>
                {
                    Time.timeScale = 1f;
                    SceneManager.LoadScene("Menu");
                }
            ),

            new PopupOption(
                "Salir del juego",
                () =>
                {
                    Time.timeScale = 1f;
                    Application.Quit();
                }
            )
        };

        OptionPopupManager.Instance.ShowPopup(
            "PAUSA\n\nSelecciona una opcion:",
            opciones
        );
    }
}
