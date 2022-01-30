using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGatherer : MonoBehaviour
{
    void Start()
    {
        var dropdown = GetComponent<TMP_Dropdown>();
        dropdown.options.Clear();

        for (int i = 2; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData($"Level_0{i-2}"));
        }


        // var info = new DirectoryInfo("Assets/Scenes/FINAL/Levels");
        // var fileInfo = info.GetFiles();
        // List<string> list = fileInfo.Where(g => !g.Name.Contains("meta")).Select(f => f.Name.Split('.').FirstOrDefault()).ToList();
        // var dropdown = GetComponent<TMP_Dropdown>();
        // dropdown.options.Clear();
        // foreach (string option in list)
        // {
        //     dropdown.options.Add(new TMP_Dropdown.OptionData(option));
        // }
    }
}