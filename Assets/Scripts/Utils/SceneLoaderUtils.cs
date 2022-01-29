using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Utils
{
    public static class SceneLoaderUtils
    {
        public static void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public static void LoadNextScene()
        {
            string[] values = SceneManager.GetActiveScene().name.Split('_');
            int currentNumber = int.Parse(values[1]);
            
            int newNumber = (currentNumber+1);
            string result = newNumber.ToString();
            if (newNumber < 10)
                result = "0" + newNumber;
            
            LoadScene(values[0] + "_" + result);
        }
    }
}