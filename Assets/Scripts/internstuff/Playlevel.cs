using Assets.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Playlevel : MonoBehaviour
{
    [SerializeField]
    private TMP_Dropdown dropdown;


    public void LoadLevel()
    {
        SceneLoaderUtils.LoadScene(dropdown.options[dropdown.value].text);
    }
}