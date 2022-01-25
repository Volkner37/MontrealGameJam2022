using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneLoaderUtils.LoadScene("MAIN");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}