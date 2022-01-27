using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
public class SetLevelOptions : MonoBehaviour
{
    void Start()
    {
        var info = new DirectoryInfo("Assets/Scenes/FINAL/Levels");
        var fileInfo = info.GetFiles();
        List<string> list = fileInfo.Where(g => !g.Name.Contains("meta")).Select(f => f.Name.Split('.').FirstOrDefault()).ToList();
        var dropdown = GetComponent<TMP_Dropdown>();
        dropdown.options.Clear();
        foreach (string option in list)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData(option));
        }
    }
}