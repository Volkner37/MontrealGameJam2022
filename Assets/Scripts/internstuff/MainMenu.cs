using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Play button script, calls the game screen
    public void PlayGame()
    {
        SceneManager.LoadScene("MAIN");

    }

    public void QuitGame()
    {
        Application.Quit();

    }
}