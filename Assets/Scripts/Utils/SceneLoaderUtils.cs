using System;
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
            LevelChanger _levelChanger = LevelChanger.Instance;
            OnSceneLoaded.Invoke(null, new SceneLoadEventArgs(sceneName));
            _levelChanger.FadeToLevel(sceneName);
        }

        public static void LoadNextScene()
        {
            string nextScene = FindNextSceneName();
            LoadScene(nextScene);
        }

        public static int GetSceneIndex(string name)
        {
            int result =SceneUtility.GetBuildIndexByScenePath($"Assets/Scenes/FINAL/Levels/{name}.unity");
            return result < 0 ? 1 : result;
        }

        private static string FindNextSceneName()
        {
            string currentScene = SceneManager.GetActiveScene().name;
            
            string[] values = currentScene.Split('_');
            int currentNumber = int.Parse(values[1]);
            
            int newNumber = currentNumber+1;
            string result = newNumber.ToString();
            
            //Scene below 10 need a 0 before their number. (Exemple : Level_02)
            if (newNumber < 10)
                result = "0" + newNumber;

            string computedName = values[0] + "_" + result;

            int index = GetSceneIndex(computedName);

            return index == 1 ? "MainMenu" : computedName;
        }

        public static event EventHandler<SceneLoadEventArgs> OnSceneLoaded = delegate {  };

        public class SceneLoadEventArgs : EventArgs
        {
            public SceneLoadEventArgs(string name)
            { 
                Name = name;
            }
            
            public readonly string Name;
        }
    }
}