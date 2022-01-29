using System;
using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    [SerializeField] private string firstLevelName;
    public void PlayGame()
    {
        SceneLoaderUtils.LoadScene(firstLevelName);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}