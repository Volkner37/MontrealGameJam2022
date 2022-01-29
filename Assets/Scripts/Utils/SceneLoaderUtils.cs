﻿using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Utils
{
    public static class SceneLoaderUtils
    {
        const int MaxLevelNumber = 0;

        public static void LoadScene(string sceneName)
        {
            //SceneManager.LoadScene(sceneName);
            Debug.Log(sceneName);
            LevelChanger _levelChanger = LevelChanger.Instance;
            _levelChanger.FadeToLevel(sceneName);
        }

        public static void LoadNextScene()
        {
            string[] values = SceneManager.GetActiveScene().name.Split('_');
            int currentNumber = int.Parse(values[1]);
            
            int newNumber = currentNumber+1;
            string result = newNumber.ToString();
            
            //Scene below 10 need a 0 before their number. (Exemple : Level_02)
            if (newNumber < 10)
                result = "0" + newNumber;

            string nextScene = values[0] + "_" + result;
            
            int sceneIndex = SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/FINAL/Levels/{nextScene}.unity");
            
            Debug.Log($"Assets/Scenes/FINAL/Levels/{nextScene}.unity" + " INDEX OF " + sceneIndex);

            LoadScene(sceneIndex >= 0 ? nextScene : "MainMenu");
        }
    }
}