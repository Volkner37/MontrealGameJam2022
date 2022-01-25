using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Utils
{
    public static class SceneLoaderUtils 
    {
        public static void LoadScene (string sceneName)
        {
            SceneManager.LoadScene(sceneName);

        }
    }
    
}