using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles the buttons in Main Menu.
/// </summary>
public class MainMenu : MonoBehaviour
{
    /// <summary>
    /// When clicked play button, loads the game scene.
    /// </summary>
    public void PlayGame()
    {
        SceneManager.LoadScene(2);
    }

    /// <summary>
    /// When clicked setting button, loads the settings scene.
    /// </summary>
    public void Settings()
    {
        SceneManager.LoadScene(1);
    }

    /// <summary>
    /// When clicked exit button, exits the game.
    /// </summary>
    public void Exit()
    {
        Application.Quit();
    }
}
